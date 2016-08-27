// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

import fs = require('fs');
import async = require('async');

import logger = require('./logger');

export class DefaultLocalizer implements ILocalizer {
    private _defaultLocale: string;
    private botLocalePath: string;

    private static localeRequests: any = {};
    private static filesParsedMap: any = {};
    private static map: any = {};

    constructor() {        
    }

    initialize(settings?: ILocalizerSettings) {
        if (!settings) {
            return;
        }

        if (settings.botLocalePath) {
            this.botLocalePath = settings.botLocalePath.toLowerCase();
            if (this.botLocalePath.charAt(this.botLocalePath.length - 1) != '/') {
                this.botLocalePath = this.botLocalePath + "/";
            }
        } else {
            this.botLocalePath = "./"
        }
        
        if (settings.defaultLocale) {
            this.defaultLocale(settings.defaultLocale.toLowerCase());
        } else {
            this.defaultLocale("en");
        }
    }

    public defaultLocale(locale?: string): string {
        if (locale) {
            this._defaultLocale = locale;
        } else {
            return this._defaultLocale;
        }
    }   
    

    private getFallback(locale: string): string {
        if (locale) {
            var split = locale.indexOf("-");
            if (split != -1) {
                return locale.substring(0, split);
            }
        }
        return this.defaultLocale();
    }

    private parseFile(localeDir: string, filename: string, locale:string, cb:AsyncResultCallback<number>) {
        var filePath = (localeDir + "/" + filename).toLowerCase();
            
        if (DefaultLocalizer.filesParsedMap[filePath]) {
            cb(null, DefaultLocalizer.filesParsedMap[filePath])
        } else {
            logger.debug("localizer::parsing %s", filePath)
            fs.readFile(filePath, 'utf8', (err, data) => {
                if (err) {
                    cb(err, -1)
                    return;
                };
                
                var parsedEntries:any;
                try {
                    parsedEntries = JSON.parse(data);                    
                } catch (error) {
                    cb(err, -1);
                    return;
                }

                var ns = filename.substring(0, filename.length - 5).toLowerCase();            
                var count = this.loadInMap(locale, ns == "index" ? null : ns, parsedEntries);
                DefaultLocalizer.filesParsedMap[filePath] = count;
                cb(null, count);                                
            });
        }
    }

    private loadInMap(locale: string, ns: string, entries:any): number {
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
    }

    private createKey(ns: string, msgid: string) : string {
        var escapedMsgId = this.escapeKey(msgid);
        var prepend = "";
        if (ns) {
            prepend = ns + ":";
        }
        return prepend + msgid;
    }

    private escapeKey(key: string): string {
        return key.replace(/:/g, "--").toLowerCase();
    }

    private randomizeValue(a: Array<any>): string {
        var i = Math.floor(Math.random() * a.length);
        return this.getValue(a[i]);
    }

    private getValue(text: any) : string {
        if (typeof text == "string") {
            return text;
        } else if (Array.isArray(text)) {
            return this.randomizeValue(text);
        } else {
            return JSON.stringify(text);
        }
    }

    private loadLocale(localeDirPath: string, locale: string, cb:AsyncResultCallback<number>) : void {
        if (!locale) {
            cb(null, -1);
            return
        }

        var path = localeDirPath + locale;
                
        fs.access(path, (err) => {
            if (err && err.code === 'ENOENT') {
                logger.warn(null, "localizer::couldn't find directory: %s", path);                                
                cb(null, -1);
            } else if (err) {
                cb(err, -1);
            } else {
                fs.readdir(path, (err, files) => {
                    logger.debug("localizer::in directory: %s", path);                                
                
                    // directory exists, but could not enumerate it                        
                    if (err) { 
                        cb(err, 0);
                    }

                    var entryCount = 0;
                    
                    async.each(files, (file, callback) => {
                        logger.debug("localizer::in file: %s", file);                                
                
                        if (file.substring(file.length - 5).toLowerCase() == ".json") {
                            this.parseFile(path, file, locale, (e:Error, c:number) => {
                                entryCount += c;
                                callback();
                            });
                        } else {
                            callback();
                        }
                    }, function(err) {
                        if (err) {
                            cb(err, -1);
                        } else {
                            logger.debug("localizer::directory complete: %s", path);                                               
                            cb(null, entryCount);                        
                        }
                    })                
                });
            }            
        });
    }
    
    public load(locale: string, done: ErrorCallback): void {
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
            // load the en locale from the lib folder -- system default
            (cb:AsyncResultCallback<number>) => { 
                this.loadLocale(__dirname + "/locale/" , "en", cb); 
            },
            
            // load the default locale from the lib folder
            (cb:AsyncResultCallback<number>) => { 
                this.loadLocale(__dirname + "/locale/" , this.defaultLocale(), cb); 
            },

            // load the fallback locale the lib folder, if different from default
            (cb:AsyncResultCallback<number>) => { 
                if (this.defaultLocale() != fb) {
                    this.loadLocale(__dirname + "/locale/", fb, cb); 
                } else {
                    cb(null, 0);
                }
            },
            
            // load the requested locale from the lib folder
            (cb:AsyncResultCallback<number>) => {  
                if (this.defaultLocale() != locale && fb != locale) {
                    this.loadLocale(__dirname + "/locale/", locale, cb); 
                } else {                    
                    cb(null, 0);
                }
            },

            // load the default locale from the botLocale folder if it exists 
            (cb:AsyncResultCallback<number>) => { 
                if (this.botLocalePath) {
                    this.loadLocale(this.botLocalePath, this.defaultLocale(), cb); 
                } else {
                    cb(null, 0);
                } 
            },
            
            // load the fallback locale the lib folder, if different from default
            (cb:AsyncResultCallback<number>) => { 
                if (this.botLocalePath && this.defaultLocale() != fb) {
                    this.loadLocale(this.botLocalePath, fb, cb); 
                } else {                                
                    cb(null, 0);
                }
            },
            
            // load the requested locale from the lib folder
            (cb:AsyncResultCallback<number>) => { 
                if (this.botLocalePath && this.defaultLocale() != locale && fb != locale) {
                    this.loadLocale(this.botLocalePath, locale, cb); 
                }  else {
                    cb(null, 0);
                } 
            },
            
        ],
        (err, results) => {
            if (err) {
                done(err);
            } else {
                DefaultLocalizer.localeRequests[localeRequestKey] = true;
                logger.debug("localizer::loaded requested locale: %s", localeRequestKey);                            
                done();
            }
        });
    }

    public trygettext(locale: string, msgid: string, namespace: string): string {
        if (locale) {
            locale = locale.toLowerCase();
        } else {
            locale = "";
        }

        if (namespace) {
            namespace = namespace.toLocaleLowerCase();
        } else {
            namespace = "";
        }
        
        var fb = this.getFallback(locale);
        var processedKey = this.createKey(namespace, msgid);
        
        logger.debug("localizer::trygettext locale:%s msgid:%s ns:%s fb:%s key:%s", locale, msgid, namespace, fb, processedKey);                                                                               
        var text:string = null;
        if (DefaultLocalizer.map[locale] && DefaultLocalizer.map[locale][processedKey]) {
            text = DefaultLocalizer.map[locale][processedKey];
        } else if (DefaultLocalizer.map[fb] && DefaultLocalizer.map[fb][processedKey]) {
            text = DefaultLocalizer.map[fb][processedKey];
        } else if (DefaultLocalizer.map[this.defaultLocale()] && DefaultLocalizer.map[this.defaultLocale()][processedKey]) {
            text = DefaultLocalizer.map[this.defaultLocale()][processedKey];
        }

        if (text) {
            text = this.getValue(text);
        }
        
        logger.debug("localizer::trygettext returning: %s", text);
        return text;
    }

    public gettext(locale: string, msgid: string, namespace: string): string {
        logger.debug("localizer::gettext locale:%s msgid:%s ns:%s", locale, msgid, namespace);                                                                               
              
        var t = this.trygettext(locale, msgid, namespace);
        if (!t) {
            t = msgid;
        }
        logger.debug("localizer::gettext returning: %s", t);        
        return t;
    } 

    public ngettext(locale: string, msgid: string, msgid_plural: string, count: number, namespace: string): string {
        logger.debug("localizer::ngettext locale:%s count: %d, msgid:%s msgid_plural:%s ns:%s", locale, count, msgid, msgid_plural, namespace);                                                                                       
        
        var t = "";
        if (count == 1) {
	        t = this.trygettext(locale, msgid, namespace) || msgid;
        } else {
            // 0 or more than 1
	        t = this.trygettext(locale, msgid_plural, namespace) || msgid_plural;            
        }
        return t;
    }   
}
