const { join } = require("path");

module.exports = {
    initialize: () => {
        process.env.port = 8081;
        process.env.publicDir = join(__dirname, "/../../client");
    }
}