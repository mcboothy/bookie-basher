const puppeteer = require('puppeteer-core');

class Browser {
    constructor() {
        this.viewportWidth = 1024;
        this.viewportHeight = 600;
        this.userAgent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 80.0.3987.87 Safari / 537.36';
        //this.userAgent = 'Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A404 Safari/601.1';
        //this.userAgent = 'Mozilla/5.0 (X11; CrOS armv7l 6946.86.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.91 Safari/537.36';
    }

    async init(chromePath) {
        this.chromePath = chromePath;
        this.browser = await puppeteer.launch({
            headless: true,
            executablePath: chromePath,
            args: ["--no-sandbox", "--disable-setuid-sandbox", '--disable-dev-shm-usage', ]
        });
    }

    async newPage() {
        var page = await this.browser.newPage();

        page.on('error', error => {
            process.stdout.write(`-- ${error}\n`);
        });

        //page.on('console', message => {
        //    process.stdout.write(`?? ${message.text()}\n`);
        //});

        await page.setViewport({ width: this.viewportWidth, height: this.viewportHeight });
        await page.setUserAgent(this.userAgent);

        return page;
    }

    async shutdown() {        
        await this.browser.close();
    }
}

module.exports = new Browser();
