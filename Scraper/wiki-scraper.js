'use strict';
var browser = require('./browser');
const BASE_URL = "https://en.soccerwiki.org/";

class WikiScraper {

    async scrapeTeams(request) {
        var page = await browser.newPage();

        try {
            await page.goto(BASE_URL + "league.php?leagueid=" + request.Competition.SoccerWikiId, { timeout: 300000 });
            await page.addScriptTag({ url: 'https://code.jquery.com/jquery-3.2.1.min.js' });
            await page.waitForSelector('.factfileBorder');

            var results = await page.evaluate(() => {
                try {
                    var data = [];

                    $("table[summary='All clubs']").each(function () {
                        $(this).children('tbody').children().each(function () {
                            var img = $(this).children(":nth-child(1)")
                                .children(':first');
                            var name = $(this).children(":nth-child(2)");

                            data.push({
                                Url: img.attr('src'),
                                Name: name.text()
                            });
                        });
                    });

                    return data;
                } catch (err) {
                    console.error('ERROR - ' + err);
                }
            });

            return results;

        } finally {
            await page.close();
        }
    }
}

module.exports = new WikiScraper();