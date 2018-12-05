const http = require("http");
const { inspect } = require("util");
const { extname, normalize } = require("path");
const { stat, createReadStream } = require("fs");

const Entities = require('html-entities').AllHtmlEntities;
const entities = new Entities();

const port = process.env.port;
const mimeTypes = {
    html: "text/html",
    jpeg: "image/jpeg",
    jpg: "image/jpeg",
    png: "image/png",
    js: "text/javascript",
    css: "text/css",
    svg: "image/svg+xml",
    txt: "text/plain",
    xml: {
        sitemap: "application/xml",
        rss: "application/rss+xml"
    },
    ggb: "application-x/geogebra-file",
    woff: "font/woff",
    map: "application/json" // https://stackoverflow.com/questions/44183545/correct-mime-type-for-css-map-files
};

const serveFile = (filePath, req, res) => {
    stat(filePath, err => {
        if(err) {
            serve404(req, res);
        }
        else {
            let ext = extname(filePath).split(".")[1];
            let mimeType = "";
            if(ext !== "xml") {
                mimeType = mimeTypes[ext];
            }
            else {
                let fileName = filePath.split("/").pop().replace(`.${ext}`, "");
                mimeType = mimeTypes.xml[fileName];
            }
            res.setHeader("Content-Type", mimeType);
            res.statusCode = 200;
            createReadStream(filePath).pipe(res);
        }
    });
}

const serve404 = (req, res) => {
    var message = `404: ${req.url} not found`;

    console.log(message);
    
    res.writeHead(404, mimeTypes.html);
    res.end(message);
}


const handleFileRequest = (req, res) => {
    let urn = req.url;    
    let filePath = `${process.env.publicDir}${normalize(urn)}`;

    if(urn === "/") {
        filePath = `${process.env.publicDir}/index.html`;
    }
    
    console.log(filePath);
    serveFile(filePath, req, res);
}

const start = () => {
    let server = http.createServer((req, res) => {
        try {
            handleFileRequest(req, res);
        }
        catch(ex) {
            var errorMessage = `<h1>${(new Date()).toString()}</h1> <br><hr>  ${entities.encode(inspect(ex))}`;
            errorMessage = errorMessage.replace(/\\n/g, "<br>").replace(/ /g, "&nbsp;").replace(/\\\\/g, "\\");
            console.log(`500: Server Errror ${errorMessage}`);
            res.writeHead(500, mimeTypes.html);
            res.end(errorMessage);
        }
    });
    server.listen(port, (err) => {
        if(err) {
            throw err;
        }
        console.log(`server is listening on ${port}`);
    });
};

module.exports = {
    start: start
};