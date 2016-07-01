var sprintf = require('sprintf-js');
function clone(obj) {
    var cpy = {};
    if (obj) {
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                cpy[key] = obj[key];
            }
        }
    }
    return cpy;
}
exports.clone = clone;
function toDate8601(date) {
    return sprintf.sprintf('%04d-%02d-%02d', date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
exports.toDate8601 = toDate8601;
