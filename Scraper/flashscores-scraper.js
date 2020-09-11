'use strict';
var browser = require('./browser');
const BASE_URL = "https://www.flashscore.co.uk/";
const FULL_NAME_URL_URL = "https://www.flashscore.com/";

class FlashScoresScraper {
    constructor() {
        this.idleSpan = 2000;
        this.sleep = 500;
        this.lastRequest = Date.now();
    }

    async scrapeFixtures(request) {
        var results = await Promise.all(
            [
                this.getFixtures(request),
                this.getResults(request)
            ]);

        if (results[0].length == 0 && results[0].length == 0)
            return null;

        var liveFixtures = await this.getLiveFixtures(request);
        return results[0].concat(results[1]).concat(liveFixtures);
    }

    async scrapeMatch(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "match/" + request.MatchId + "/#match-summary", { timeout: 300000 });
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });


            await page.waitForFunction(() => document.querySelector('.info-status.mstat').innerText !== '', { timeout: 10000 });
            var status = await page.$eval('.info-status.mstat', elem => elem.innerText);

            if (status !== 'Finished') {
                console.error('STATUS scrapeMatch - ' + status);
                return null;
            }

            await page.waitForSelector('.detailMS');

            return await page.evaluate(() => {
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
                    console.error('ERROR scrapeMatch - ' + err);
                }
            });
        } finally {
            await page.close();
        }
    }

    async scrapeTeams(request, isFull) {
        var page = await browser.newPage();

        try {
            var url = BASE_URL + "football/" + this.getUrl(request) + "/standings";

            if (isFull === true) {
                url = FULL_NAME_URL_URL + "football/" + this.getUrl(request) + "/standings/";
            }

            await page.goto(url, { timeout: 300000 });
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

            await page.waitForSelector('#tabitem-table');

            try {
                // greek league has a championship stage with reduced teams
                // ensure we are looking at the standings for the main league
                await page.waitForSelector('.ifmenu.bubble.stages-menu', { timeout: 1000 });
                await page.evaluate(() => {
                    try {
                        $("a:contains('Main')")[0].click();
                    } catch (err) {
                        console.error('ERROR scrapeTeams - ' + err);
                    }
                });
                await page.waitForNavigation();
            } catch (err) {
                if (!err.message.includes('waiting for selector ".ifmenu.bubble.stages-menu"'))
                    throw err;
            }

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
                    console.error('ERROR scrapeTeams - ' + err);
                }
            });

            return results;

        } finally {
            await page.close();
        }
    }

    async scrapeStandings(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "football/" + this.getUrl(request) + "/standings");
            var leagues = [];

            await page.waitForSelector('#tabitem-table');
            await page.$eval('#tabitem-table', elem => elem.click());
            await page.waitForSelector('#table-type-1');

            leagues.push({
                Type: 'Overall',
                LeaguePositions: await this.scrapeLeague(page, { Selector: '#table-type-1' })
            });

            await page.waitForSelector('#tabitem-table-home');
            await page.$eval('#tabitem-table-home', elem => elem.click());
            await page.waitForSelector('#table-type-2');

            leagues.push({
                Type: 'Home',
                LeaguePositions: await this.scrapeLeague(page, { Selector: '#table-type-2' })
            });

            await page.waitForSelector('#tabitem-table-away');
            await page.$eval('#tabitem-table-away', elem => elem.click());
            await page.waitForSelector('#table-type-3');

            leagues.push({
                Type: 'Away',
                LeaguePositions: await this.scrapeLeague(page, { Selector: "#table-type-3" })
            });

            await page.waitForSelector('#tabitem-form-overall');
            await page.$eval('#tabitem-form-overall', elem => elem.click());

            var tableNames = await findTableNumbers(page, {
                Selector: '#glib-stats-submenu-form-overall',
                TableType: '#table-type-5-'
            });

            for (var overallIndex = 0; overallIndex < tableNames.length; overallIndex++) {
                await page.waitForSelector(tableNames[overallIndex].Selector);

                leagues.push({
                    Type: 'Form-Overall-' + tableNames[overallIndex].Number,
                    LeaguePositions: await this.scrapeLeague(page, { Selector: tableNames[overallIndex].Selector })
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
                    LeaguePositions: await this.scrapeLeague(page, { Selector: tableNames[homeIndex].Selector })
                });
            }

            await page.waitFor(2000);
            await page.$eval('#tabitem-form-away', elem => elem.click());

            tableNames = await this.findTableNumbers(page, {
                Selector: '#glib-stats-submenu-form-away',
                TableType: '#table-type-9-'
            });

            for (var awayIndex = 0; awayIndex < tableNames.length; awayIndex++) {
                await page.waitForSelector(tableNames[awayIndex].Selector);

                leagues.push({
                    Type: 'Form-Away-' + tableNames[awayIndex].Number,
                    LeaguePositions: await this.scrapeLeague(page, { Selector: tableNames[awayIndex].Selector })
                });
            }

            return leagues;
        } finally {
            await page.close();
        }
    }

    async scrapeLeague(page, opts) {
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

    async getFixtures(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "football/" + this.getUrl(request) + "/fixtures", { timeout: 300000 });
            await this.waitForIdle(page);
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

            await page.waitForSelector('#live-table');

            var noMatch = page.waitForSelector('#no-match-found').then(() => false);
            var dataAvailable = page.waitForSelector('.sportName.soccer').then(() => true);

            var result = await Promise.race([noMatch, dataAvailable]);

            if (result == false) {
                return [];
            }

            await this.waitForAll(page);

            var results = await page.evaluate(() => {
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
                    console.error('ERROR scrapeFixtures - ' + err);
                }
            });

            return results;

        } finally {
            await page.close();
        }
    }

    async getResults(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "football/" + this.getUrl(request) + "/results", { timeout: 300000 });
            await this.waitForIdle(page);
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

            await page.waitForSelector('#live-table');

            var noMatch = page.waitForSelector('#no-match-found').then(() => false);
            var dataAvailable = page.waitForSelector('.sportName.soccer').then(() => true);

            var result = await Promise.race([noMatch, dataAvailable]);

            if (result == false) {
                return [];
            }

            await this.waitForAll(page);

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

            return results;

        } finally {
            await page.close();
        }
    }

    async getLiveFixtures(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "football/" + this.getUrl(request), { timeout: 300000 });
            await this.waitForIdle(page);
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });

            await page.waitForSelector('#live-table');
            await page.waitForSelector('.sportName.soccer');

            var results = await page.evaluate(() => {
                try {
                    var data = [];

                    $('.event__match').each(function () {
                        if ($(this).hasClass('event__match--live')) {
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
                    console.error('ERROR - scrapeFixtures ' + err);
                }
            });

            for (var index = 0; index < results.length; index++) {
                await page.goto(BASE_URL + "match/" + results[index].FSMatchID + "/#match-summary", { timeout: 300000 });
                await page.waitForSelector('#utime');
                var time = await page.$eval('#utime', e => e.innerText);
                var parts = time.split(' ');
                var dateParts = parts[0].split('.');
                var date = dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0];
                results[index].DateTime = new Date(date + " " + parts[1]);
            }

            return results;

        } finally {
            await page.close();
        }
    }

    async findTableNumbers(page, opts) {
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

    async waitForAll(page) {
        while (await page.$('.event__more') !== null) {
            await page.setRequestInterception(true);
            page.on('request', this.registerRequest);

            // the balank catch is to prevent race condition where 
            // await page.$('.event__more') isn't null
            // but click('.event__more') raises an error that no node exists
            page.click('.event__more')
                .catch((err) => { });

            this.wait(page);

            page.removeListener('request', this.registerRequest);
            await page.setRequestInterception(false);
        }
    }

    async waitForIdle(page) {
        await page.setRequestInterception(true);
        page.on('request', this.registerRequest);

        this.wait(page);

        page.removeListener('request', this.registerRequest);
        await page.setRequestInterception(false);
    }

    async wait(page) {
        var diff = Date.now() - this.lastRequest;

        while (diff < this.idleSpan)
            await page.waitFor(this.sleep);
    }

    getUrl(season) {
        return season.Competition.FlashScoreUrl + '-' + season.Year.replace('/', '-');
    }

    registerRequest(request) {
        request.continue();
        this.lastRequest = Date.now();
    }
}

module.exports = new FlashScoresScraper();