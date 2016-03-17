import sprintf = require('sprintf-js');

export function clone(obj: any): any {
    var cpy: any = {};
    if (obj) {
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                cpy[key] = obj[key];
            }
        }
    }
    return cpy;
}

export function toDate8601(date: Date): string {
    return sprintf.sprintf('%04d-%02d-%02d', date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}