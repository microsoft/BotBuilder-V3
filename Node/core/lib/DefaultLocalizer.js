"use strict";
var fs = require('fs');
var async = require('async');
var logger = require('./logger');
var DefaultLocalizer = (function () {
    function DefaultLocalizer() {
    }
    DefaultLocalizer.prototype.initialize = function (settings) {
        if (!settings) {
            return;
        }
        if (settings.botLocalePath) {
            this.botLocalePath = settings.botLocalePath.toLowerCase();
            if (this.botLocalePath.charAt(this.botLocalePath.length - 1) != '/') {
                this.botLocalePath = this.botLocalePath + "/";
            }
        }
        else {
            this.botLocalePath = "./";
        }
        if (settings.defaultLocale) {
            this.defaultLocale(settings.defaultLocale.toLowerCase());
        }
        else {
            this.defaultLocale("en");
        }
    };
    DefaultLocalizer.prototype.defaultLocale = function (locale) {
        if (locale) {
            this._defaultLocale = locale;
        }
        else {
            return this._defaultLocale;
        }
    };
    DefaultLocalizer.prototype.getFallback = function (locale) {
        if (locale) {
            var split = locale.indexOf("-");
            if (split != -1) {
                return locale.substring(0, split);
            }
        }
        return this.defaultLocale();
    };
    DefaultLocalizer.prototype.parseFile = function (localeDir, filename, locale, cb) {
        var _this = this;
        var filePath = (localeDir + "/" + filename).toLowerCase();
        if (DefaultLocalizer.filesParsedMap[filePath]) {
            cb(null, DefaultLocalizer.filesParsedMap[filePath]);
        }
        else {
            logger.debug("localizer::parsing %s", filePath);
            fs.readFile(filePath, 'utf8', function (err, data) {
                if (err) {
                    cb(err, -1);
                    return;
                }
                ;
                var parsedEntries;
                try {
                    parsedEntries = JSON.parse(data);
                }
                catch (error) {
                    cb(err, -1);
                    return;
                }
                var ns = filename.substring(0, filename.length - 5).toLowerCase();
                var count = _this.loadInMap(locale, ns == "index" ? null : ns, parsedEntries);
                DefaultLocalizer.filesParsedMap[filePath] = count;
                cb(null, count);
            });
        }
    };
    DefaultLocalizer.prototype.loadInMap = function (locale, ns, entries) {
        if (!DefaultLocalizer.map[locale]) {
            DefaultLocalizer.map[locale] = {};
        }
        var i = 0;
        for (var key in entries) {
            var processedKey = this.createKey(ns, key);
            DefaultLocalizer.map[locale][processedKey] = entries[key];
            ++i;
        }
        return i;
    };
    DefaultLocalizer.prototype.createKey = function (ns, msgid) {
        var escapedMsgId = this.escapeKey(msgid);
        var prepend = "";
        if (ns) {
            prepend = ns + ":";
        }
        return prepend + msgid;
    };
    DefaultLocalizer.prototype.escapeKey = function (key) {
        return key.replace(/:/g, "--").toLowerCase();
    };
    DefaultLocalizer.prototype.randomizeValue = function (a) {
        var i = Math.floor(Math.random() * a.length);
        return this.getValue(a[i]);
    };
    DefaultLocalizer.prototype.getValue = function (text) {
        if (typeof text == "string") {
            return text;
        }
        else if (Array.isArray(text)) {
            return this.randomizeValue(text);
        }
        else {
            return JSON.stringify(text);
        }
    };
    DefaultLocalizer.prototype.loadLocale = function (localeDirPath, locale, cb) {
        var _this = this;
        if (!locale) {
            cb(null, -1);
            return;
        }
        var path = localeDirPath + locale;
        fs.access(path, function (err) {
            if (err && err.code === 'ENOENT') {
                logger.warn(null, "localizer::couldn't find directory: %s", path);
                cb(null, -1);
            }
            else if (err) {
                cb(err, -1);
            }
            else {
                fs.readdir(path, function (err, files) {
                    logger.debug("localizer::in directory: %s", path);
                    if (err) {
                        cb(err, 0);
                    }
                    var entryCount = 0;
                    async.each(files, function (file, callback) {
                        logger.debug("localizer::in file: %s", file);
                        if (file.substring(file.length - 5).toLowerCase() == ".json") {
                            _this.parseFile(path, file, locale, function (e, c) {
                                entryCount += c;
                                callback();
                            });
                        }
                        else {
                            callback();
                        }
                    }, function (err) {
                        if (err) {
                            cb(err, -1);
                        }
                        else {
                            logger.debug("localizer::directory complete: %s", path);
                            cb(null, entryCount);
                        }
                    });
                });
            }
        });
    };
    DefaultLocalizer.prototype.load = function (locale, done) {
        var _this = this;
        if (locale) {
            locale = locale.toLowerCase();
        }
        var localeRequestKey = locale || "_*_";
        logger.debug("localizer::load requested for: %s", localeRequestKey);
        if (DefaultLocalizer.localeRequests[localeRequestKey]) {
            logger.debug("localizer::already loaded requested locale: %s", localeRequestKey);
            done(null);
            return;
        }
        var fb = this.getFallback(locale);
        async.series([
            function (cb) {
                _this.loadLocale(__dirname + "/locale/", "en", cb);
            },
            function (cb) {
                _this.loadLocale(__dirname + "/locale/", _this.defaultLocale(), cb);
            },
            function (cb) {
                if (_this.defaultLocale() != fb) {
                    _this.loadLocale(__dirname + "/locale/", fb, cb);
                }
                else {
                    cb(null, 0);
                }
            },
            function (cb) {
                if (_this.defaultLocale() != locale && fb != locale) {
                    _this.loadLocale(__dirname + "/locale/", locale, cb);
                }
                else {
                    cb(null, 0);
                }
            },
            function (cb) {
                if (_this.botLocalePath) {
                    _this.loadLocale(_this.botLocalePath, _this.defaultLocale(), cb);
                }
                else {
                    cb(null, 0);
                }
            },
            function (cb) {
                if (_this.botLocalePath && _this.defaultLocale() != fb) {
                    _this.loadLocale(_this.botLocalePath, fb, cb);
                }
                else {
                    cb(null, 0);
                }
            },
            function (cb) {
                if (_this.botLocalePath && _this.defaultLocale() != locale && fb != locale) {
                    _this.loadLocale(_this.botLocalePath, locale, cb);
                }
                else {
                    cb(null, 0);
                }
            },
        ], function (err, results) {
            if (err) {
                done(err);
            }
            else {
                DefaultLocalizer.localeRequests[localeRequestKey] = true;
                logger.debug("localizer::loaded requested locale: %s", localeRequestKey);
                done();
            }
        });
    };
    DefaultLocalizer.prototype.trygettext = function (locale, msgid, namespace) {
        if (locale) {
            locale = locale.toLowerCase();
        }
        else {
            locale = "";
        }
        if (namespace) {
            namespace = namespace.toLocaleLowerCase();
        }
        else {
            namespace = "";
        }
        var fb = this.getFallback(locale);
        var processedKey = this.createKey(namespace, msgid);
        logger.debug("localizer::trygettext locale:%s msgid:%s ns:%s fb:%s key:%s", locale, msgid, namespace, fb, processedKey);
        var text = null;
        if (DefaultLocalizer.map[locale] && DefaultLocalizer.map[locale][processedKey]) {
            text = DefaultLocalizer.map[locale][processedKey];
        }
        else if (DefaultLocalizer.map[fb] && DefaultLocalizer.map[fb][processedKey]) {
            text = DefaultLocalizer.map[fb][processedKey];
        }
        else if (DefaultLocalizer.map[this.defaultLocale()] && DefaultLocalizer.map[this.defaultLocale()][processedKey]) {
            text = DefaultLocalizer.map[this.defaultLocale()][processedKey];
        }
        if (text) {
            text = this.getValue(text);
        }
        logger.debug("localizer::trygettext returning: %s", text);
        return text;
    };
    DefaultLocalizer.prototype.gettext = function (locale, msgid, namespace) {
        logger.debug("localizer::gettext locale:%s msgid:%s ns:%s", locale, msgid, namespace);
        var t = this.trygettext(locale, msgid, namespace);
        if (!t) {
            t = msgid;
        }
        logger.debug("localizer::gettext returning: %s", t);
        return t;
    };
    DefaultLocalizer.prototype.ngettext = function (locale, msgid, msgid_plural, count, namespace) {
        logger.debug("localizer::ngettext locale:%s count: %d, msgid:%s msgid_plural:%s ns:%s", locale, count, msgid, msgid_plural, namespace);
        var t = "";
        if (count == 1) {
            t = this.trygettext(locale, msgid, namespace) || msgid;
        }
        else {
            t = this.trygettext(locale, msgid_plural, namespace) || msgid_plural;
        }
        return t;
    };
    DefaultLocalizer.localeRequests = {};
    DefaultLocalizer.filesParsedMap = {};
    DefaultLocalizer.map = {};
    return DefaultLocalizer;
}());
exports.DefaultLocalizer = DefaultLocalizer;
