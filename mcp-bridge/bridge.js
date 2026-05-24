const express = require('express');
const puppeteer = require('puppeteer');
const app = express();
app.use(express.json());

const PORT = 3001;

// 1. List available tools
app.get('/tools', (req, res) => {
    const tools = [
        {
            name: "puppeteer_browse",
            description: "Opens a real browser, navigates to a URL, and returns the text content of the page."
        }
    ];
    res.json(tools);
});

// 2. Invoke the tool
app.post('/invoke', async (req, res) => {
    const { toolName, input } = req.body;
    console.log(`[MCP] Invoking ${toolName} with input: ${input}`);

    if (toolName === 'puppeteer_browse') {
        try {
            const browser = await puppeteer.launch({ headless: "new" });
            const page = await browser.newPage();
            
            // Clean the input (sometimes models send extra quotes)
            const url = input.replace(/['"]/g, '').trim();
            
            console.log(`[Puppeteer] Navigating to: ${url}`);
            await page.goto(url, { waitUntil: 'networkidle2', timeout: 30000 });
            
            const text = await page.evaluate(() => document.body.innerText);
            await browser.close();

            res.json({ output: text.substring(0, 2000) + "..." }); // Limit size for local LLMs
        } catch (err) {
            console.error(err);
            res.status(500).json({ output: `Error: ${err.message}` });
        }
    } else {
        res.status(404).json({ output: "Tool not found" });
    }
});

app.listen(PORT, () => {
    console.log(`🚀 Puppeteer MCP Bridge running at http://localhost:${PORT}`);
    console.log(`- GET /tools to list tools`);
    console.log(`- POST /invoke to run them`);
});
