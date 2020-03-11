'use strict';
const puppeteer = require('puppeteer-core');
const BASE_URL = "https://www.flashscores.co.uk/"; 

var browser; 
var page; 

const idleSpan = 2000;
const sleep = 500;

let lastRequest = Date.now();

var init = async function (chromePath) {
    browser = await puppeteer.launch({
        headless: true,
        executablePath: chromePath,
        args: ["--no-sandbox", "--disable-setuid-sandbox"]
    });

    page = await browser.newPage();

    page.on('error', error => {
        process.stdout.write(`❌ ${error}\n`);
    });
    page.on('console', message => {
        process.stdout.write(`👉 ${message.text()}\n`);
    });

    await page.setViewport({ width: 1024, height: 600 })
    //await page.setUserAgent('Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A404 Safari/601.1')
    //await page.setUserAgent('Mozilla/5.0 (X11; CrOS armv7l 6946.86.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.91 Safari/537.36');
    await page.setUserAgent('Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 80.0.3987.87 Safari / 537.36');
};

var shutdown = async function () {
    await browser.close();
};

var scrapeTeams = async function (request) {
    const page = await createPage("football/" + request.Competition.Url + "/standings");

    await page.waitForSelector('#tabitem-table');
    await page.$eval('#tabitem-table', elem => elem.click());
    await page.waitForSelector('#table-type-1');

    await page.waitForSelector('.table__body');

    var results = await page.evaluate(() => {
        try {
            var data = [];
            $('.table__cell--col_participant_name').each(function () {
                var url = $(this).find('.team-logo')
                    .css('background-image')
                    .replace('url(', '')
                    .replace(')', '')
                    .replace(/\"/gi, "");

                var name = $(this).find('.team_name_span').text();

                if ($(this).find('.team_name_span').find('.glib-live-rank').length > 0) {
                    name = $(this).find('.team_name_span')
                                      .find('a').text();
                }

                data.push({
                    Name: name,
                    URL: url,
                });
            });

            return data; 
        } catch (err) {
            console.error('ERROR - ' + err);
        }
    });

    return results;
};

var scrapeFixtures = async function (request) {
    var page = await createPage("football/" + request.Competition.Url + "");

    await page.waitForSelector('.sportName.soccer');

    var liveFixtures = await page.evaluate(() => {
        try {
            var data = [];

            $('.event__match').each(function () {
                if ($(this).hasClass('event__match--live')){
                    var id = $(this).attr('id');
                    data.push({
                        FSMatchID: id.substring(id.lastIndexOf('_') + 1),
                        HomeTeam: $(this).find('.event__participant--home').text(),
                        AwayTeam: $(this).find('.event__participant--away').text(),
                        DateTime: null,
                    });
                }
            });

            return data;
        } catch (err) {
            console.error('ERROR - ' + err);
        }
    });

    for (var index = 0; index < liveFixtures.length; index++) {
        await page.goto(BASE_URL + "match/" + liveFixtures[index].FSMatchID + "/#match-summary");
        await page.waitForSelector('#utime');
        var time = await page.$eval('#utime', e => e.innerText);
        var parts = time.split(' ');
        var dateParts = parts[0].split('.');
        var date = dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0];
        liveFixtures[index].DateTime = new Date(date + " " + parts[1]);
    }


    await page.goto(BASE_URL + "football/" + request.Competition.Url + "/fixtures");
    await waitForIdle(page);
    await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

    await page.waitForSelector('.event--fixtures');

    await waitForAll(page);

    var fixtures = await page.evaluate(() => {
        try {
            var data = [];

            var dateNow = new Date();

            $('.event__match').each(function () {
                var dateTime = $(this).find('.event__time').text();
                var id = $(this).attr('id');
                var postponed = false;

                if ($(this).find('.event__time').find('.lineThrough').length > 0) {
                    dateTime = $(this).find('.event__time')
                                      .find('.lineThrough').text();
                    postponed = true;
                }

                var dateTimeParts = dateTime.split(' ');
                var dateParts = dateTimeParts[0].split('.');

                var day = parseInt(dateParts[0]);
                var month = parseInt(dateParts[1]);
                var year = dateNow.getFullYear();

                if (month > 7 && dateNow.getMonth() < 7)
                    year = dateNow.getFullYear() - 1;
                if (month < 7 && dateNow.getMonth() > 7)
                    year = dateNow.getFullYear() + 1;

                var date = new Date("" + year + "-" + month + "-" + day + " " + dateTimeParts[1]);

                data.push({
                    DateTime: date.toISOString(),
                    FSMatchID: id.substring(id.lastIndexOf('_') + 1),
                    HomeTeam: $(this).find('.event__participant--home').text(),
                    AwayTeam: $(this).find('.event__participant--away').text(),
                    Postponed: postponed
                });
            });

            return data;
        } catch (err) {
            console.error('ERROR - ' + err);
        }
    });

    await page.goto(BASE_URL + "football/" + request.Competition.Url + "/results");
    await waitForIdle(page);
    await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

    await page.waitForSelector('.sportName.soccer');

    await waitForAll(page);

    var results = await page.evaluate(() => {

        var data = [];

        var dateNow = new Date();

        $('.event__match').each(function () {
            try {
                var dateTime = $(this).find('.event__time').text();
                var dateTimeParts = dateTime.split(' ');
                var dateParts = dateTimeParts[0].split('.');

                var day = parseInt(dateParts[0]);
                var month = parseInt(dateParts[1]);
                var year = dateNow.getFullYear();

                if (month > 7 && dateNow.getMonth() < 7)
                    year = dateNow.getFullYear() - 1;
                if (month < 7 && dateNow.getMonth() > 7)
                    year = dateNow.getFullYear() + 1;

                var date = new Date("" + year + "-" + month + "-" + day + " " + dateTimeParts[1]);

                count += 1;

                    var id = $(this).attr('id');

                    data.push({
                        DateTime: date.toISOString(),
                        HomeTeam: $(this).find('.event__participant--home').text(),
                        AwayTeam: $(this).find('.event__participant--away').text(),
                        FSMatchID: id.substring(id.lastIndexOf('_') + 1)
                    });
            } catch (err) {
                console.error('ERROR - ' + err);
            }
        });

        return data;
    });

    return results.concat(fixtures).concat(liveFixtures);
};

var scrapeMatch = async function (request) {
    const page = await createPage("match/" + request.MatchId + "/#match-summary");

    try {
        await page.waitForSelector('.detailMS', {timeout: 3000});
        await page.waitForFunction(() => document.querySelector('.info-status.mstat').innerText !== '');
        var status = await page.$eval('.info-status.mstat', elem => elem.innerText);

        if (status !== 'Finished') {
            console.error('STATUS - ' + status);
            return null;
        }
    } catch (err) {
        console.error('Timed out I quess? - error - ' + err);
        return null;
    }

    var results = await page.evaluate(() => {
        try {
            var first = false;
            var home = true;

            var data = {
                Home: {
                    FirstHalf: {
                        Goals: 0,
                        Cards: 0
                    },
                    SecondHalf: {
                        Goals: 0,
                        Cards: 0
                    }
                },
                Away: {
                    FirstHalf: {
                        Goals: 0,
                        Cards: 0
                    },
                    SecondHalf: {
                        Goals: 0,
                        Cards: 0
                    }
                }
            };

            $('.detailMS').children('div').each(function () {
                if ($(this).hasClass('detailMS__incidentsHeader')) {
                    if (!first)
                        first = true;
                    else
                        first = false;
                } else { // must be incidentRow
                    if ($(this).hasClass('incidentRow--away')) {
                        home = false;
                    } else {
                        home = true;
                    }

                    if ($(this).find('.soccer-ball').length ||
                        $(this).find('.soccer-ball-own').length) {
                        if (first) {
                            if (home) {
                                data.Home.FirstHalf.Goals++;
                            } else {
                                data.Away.FirstHalf.Goals++;
                            }
                        } else {
                            if (home) {
                                data.Home.SecondHalf.Goals++;
                            } else {
                                data.Away.SecondHalf.Goals++;
                            }
                        }
                    } else if ($(this).find('.y-card').length ||
                        $(this).find('.yr-card').length) {
                        if (first) {
                            if (home) {
                                data.Home.FirstHalf.Cards++;
                            } else {
                                data.Away.FirstHalf.Cards++;
                            }
                        } else {
                            if (home) {
                                data.Home.SecondHalf.Cards++;
                            } else {
                                data.Away.SecondHalf.Cards++;
                            }
                        }
                    } else if ($(this).find('.r-card').length) {
                        if (first) {
                            if (home) {
                                data.Home.FirstHalf.Cards += 2;
                            } else {
                                data.Away.FirstHalf.Cards += 2;
                            }
                        } else {
                            if (home) {
                                data.Home.SecondHalf.Cards += 2;
                            } else {
                                data.Away.SecondHalf.Cards += 2;
                            }
                        }
                    } 
                }
            });

            return data;
        } catch (err) {
            console.error('ERROR - ' + err);
        }
    });

    return results;
};

var scrapeStandings = async function (request) {
    const page = await createPage("football/" + request.Competition.Url + "/standings");

    var leagues = [];

    await page.waitForSelector('#tabitem-table');
    await page.$eval('#tabitem-table', elem => elem.click());
    await page.waitForSelector('#table-type-1');

    leagues.push({
        Type: 'Overall',
        LeaguePositions: await scrapeLeague(page, { Selector: '#table-type-1' })
    });

    await page.waitForSelector('#tabitem-table-home');
    await page.$eval('#tabitem-table-home', elem => elem.click());
    await page.waitForSelector('#table-type-2');

    leagues.push({
        Type: 'Home',
        LeaguePositions: await scrapeLeague(page, { Selector: '#table-type-2' })
    });

    await page.waitForSelector('#tabitem-table-away');
    await page.$eval('#tabitem-table-away', elem => elem.click());
    await page.waitForSelector('#table-type-3');

    leagues.push({
        Type: 'Away',
        LeaguePositions: await scrapeLeague(page, { Selector: "#table-type-3" })
    });

    await page.waitForSelector('#tabitem-form-overall');
    await page.$eval('#tabitem-form-overall', elem => elem.click());

    var tableNames = await findTableNumbers(page, {
        Selector: '#glib-stats-submenu-form-overall',
        TableType: '#table-type-5-'
    });

    for (var overallIndex = 0; overallIndex < tableNames.length; overallIndex++) {
        await page.waitForSelector(tableNames[overallIndex].Selector );

        leagues.push({
            Type: 'Form-Overall-' + tableNames[overallIndex].Number,
            LeaguePositions: await scrapeLeague(page, { Selector: tableNames[overallIndex].Selector })
        });
    }

    await page.waitFor(2000);
    await page.$eval('#tabitem-form-home', elem => elem.click());

    tableNames = await findTableNumbers(page, {
        Selector: '#glib-stats-submenu-form-home',
        TableType: '#table-type-8-'
    });

    for (var homeIndex = 0; homeIndex < tableNames.length; homeIndex++) {
        await page.waitForSelector(tableNames[homeIndex].Selector);

        leagues.push({
            Type: 'Form-Home-' + tableNames[homeIndex].Number,
            LeaguePositions: await scrapeLeague(page, { Selector: tableNames[homeIndex].Selector })
        });
    }

    await page.waitFor(2000);
    await page.$eval('#tabitem-form-away', elem => elem.click());

    tableNames = await findTableNumbers(page, {
        Selector: '#glib-stats-submenu-form-away',
        TableType: '#table-type-9-'
    });

    for (var awayIndex = 0; awayIndex < tableNames.length; awayIndex++) {
        await page.waitForSelector(tableNames[awayIndex].Selector);

        leagues.push({
            Type: 'Form-Away-' + tableNames[awayIndex].Number,
            LeaguePositions: await scrapeLeague(page, { Selector: tableNames[awayIndex].Selector })
        });
    }

    return leagues;
};

function registerRequest(request) {
    request.continue();
    lastRequest = Date.now();
}

async function waitForIdle(page) {
    await page.setRequestInterception(true);
    page.on('request', registerRequest);

    while ((lastRequest + idleSpan) > Date.now())
        await page.waitFor(sleep);

    page.removeListener('request', registerRequest);
    await page.setRequestInterception(false);
}


async function waitForAll(page) {
    while (await page.$('.event__more') !== null) {
        await page.setRequestInterception(true);
        page.on('request', registerRequest);

        // the balank catch is to prevent race condition where 
        // await page.$('.event__more') isn't null
        // but click('.event__more') raises an error that no node exists
        page.click('.event__more')
            .catch((err) => { });

        while (Date.now() - lastRequest > idleSpan)
            await page.waitFor(sleep);

        page.removeListener('request', registerRequest);
        await page.setRequestInterception(false);
    }
}

async function createPage(url) {
    await page.goto(BASE_URL + url);
    await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });
    return page;
}

async function scrapeLeague(page, opts) {
    return await page.evaluate((opts) => {
        var league = [];
        var rows = $(opts.Selector).find('.table__body').children();

        for (var n = 0; n < rows.length; n++) {
            var row = rows[n];
            league.push({
                Position: $(row.children[0]).innerText,
                TeamName: $(row.children[1]).find('.team_name_span').text(),
                MatchesPlayed: row.children[2].innerText,
                MatchesWon: row.children[3].innerText,
                MatchesDrawn: row.children[4].innerText,
                MatchesLost: row.children[5].innerText,
                GoalsForAgains: row.children[6].innerText,
                Points: row.children[7].innerText
            });
        }

        return league; 
    }, opts);
}

async function findTableNumbers(page, opts) {
    return await page.evaluate((opts) => {
        var tableNames = [];
        $(opts.Selector).find('a').each(function () {
            tableNames.push({
                Number: $(this).text().trim(),
                Selector: opts.TableType + $(this).text().trim()
            });
            $(this).click();
        });

        return tableNames;
    }, opts);
}

module.exports.init = init;
module.exports.shutdown = shutdown;

module.exports.scrapeTeams = scrapeTeams;
module.exports.scrapeFixtures = scrapeFixtures;
module.exports.scrapeMatch = scrapeMatch;
module.exports.scrapeStandings = scrapeStandings;