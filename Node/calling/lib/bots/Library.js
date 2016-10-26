"use strict";
var da = require('../dialogs/DialogAction');
var sd = require('../dialogs/SimpleDialog');
var consts = require('../consts');
var Library = (function () {
    function Library(name) {
        this.name = name;
        this.dialogs = {};
        this.libraries = {};
    }
    Library.prototype.dialog = function (id, dialog) {
        var d;
        if (dialog) {
            if (id.indexOf(':') >= 0) {
                id = id.split(':')[1];
            }
            if (this.dialogs.hasOwnProperty(id)) {
                throw new Error("Dialog[" + id + "] already exists in library[" + this.name + "].");
            }
            if (Array.isArray(dialog)) {
                d = new sd.SimpleDialog(da.waterfall(dialog));
            }
            else if (typeof dialog == 'function') {
                d = new sd.SimpleDialog(da.waterfall([dialog]));
            }
            else {
                d = dialog;
            }
            this.dialogs[id] = d;
        }
        else if (this.dialogs.hasOwnProperty(id)) {
            d = this.dialogs[id];
        }
        return d;
    };
    Library.prototype.library = function (lib) {
        var l;
        if (typeof lib === 'string') {
            if (lib == this.name) {
                l = this;
            }
            else if (this.libraries.hasOwnProperty(lib)) {
                l = this.libraries[lib];
            }
            else {
                for (var name in this.libraries) {
                    l = this.libraries[name].library(lib);
                    if (l) {
                        break;
                    }
                }
            }
        }
        else {
            l = this.libraries[lib.name] = lib;
        }
        return l;
    };
    Library.prototype.findDialog = function (libName, dialogId) {
        var d;
        var lib = this.library(libName);
        if (lib) {
            d = lib.dialog(dialogId);
        }
        return d;
    };
    return Library;
}());
exports.Library = Library;
exports.systemLib = new Library(consts.Library.system);
