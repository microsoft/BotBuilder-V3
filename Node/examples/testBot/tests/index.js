var fs = require('fs');
var path = require('path');

// Load tests
var tests = {};
var files = fs.readdirSync(__dirname);
for (var i = 0; i < files.length; i++) {
    var fn = files[i];
    if (fn.lastIndexOf('.js') == fn.length - 3 && fn !== 'index.js') {
        var f = path.join(__dirname, fn);
        var m = require(f);
        if (m.addDialogs && m.run) {
            tests[fn.substr(0, fn.length - 3)] = require(f);
        }
    }
}

// Export list of tests
module.exports = tests;
