"use strict";
var sprintf = require('sprintf-js');
function clone(obj) {
    var cpy = {};
    copyTo(obj, cpy);
    return cpy;
}
exports.clone = clone;
function copyTo(frm, to) {
    if (frm) {
        for (var key in frm) {
            if (frm.hasOwnProperty(key)) {
                if (typeof to[key] === 'function') {
                    to[key](frm[key]);
                }
                else {
                    to[key] = frm[key];
                }
            }
        }
    }
}
exports.copyTo = copyTo;
function copyFieldsTo(frm, to, fields) {
    if (frm && to) {
        fields.split('|').forEach(function (f) {
            if (frm.hasOwnProperty(f)) {
                if (typeof to[f] === 'function') {
                    to[f](frm[f]);
                }
                else {
                    to[f] = frm[f];
                }
            }
        });
    }
}
exports.copyFieldsTo = copyFieldsTo;
function moveFieldsTo(frm, to, fields) {
    if (frm && to) {
        for (var f in fields) {
            if (frm.hasOwnProperty(f)) {
                if (typeof to[f] === 'function') {
                    to[fields[f]](frm[f]);
                }
                else {
                    to[fields[f]] = frm[f];
                }
                delete frm[f];
            }
        }
    }
}
exports.moveFieldsTo = moveFieldsTo;
function toDate8601(date) {
    return sprintf.sprintf('%04d-%02d-%02d', date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}
exports.toDate8601 = toDate8601;
function fmtText(session, prompts, args) {
    var fmt = randomPrompt(prompts);
    if (session) {
        fmt = session.gettext(fmt);
    }
    return args && args.length > 0 ? sprintf.vsprintf(fmt, args) : fmt;
}
exports.fmtText = fmtText;
function randomPrompt(prompts) {
    if (Array.isArray(prompts)) {
        var i = Math.floor(Math.random() * prompts.length);
        return prompts[i];
    }
    else {
        return prompts;
    }
}
exports.randomPrompt = randomPrompt;
