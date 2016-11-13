"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var SimpleDialog_1 = require('../dialogs/SimpleDialog');
var ActionSet_1 = require('../dialogs/ActionSet');
var IntentRecognizerSet_1 = require('../dialogs/IntentRecognizerSet');
var Session_1 = require('../Session');
var consts = require('../consts');
var logger = require('../logger');
var events_1 = require('events');
var path = require('path');
var async = require('async');
var Library = (function (_super) {
    __extends(Library, _super);
    function Library(name) {
        _super.call(this);
        this.name = name;
        this.dialogs = {};
        this.libraries = {};
        this.actions = new ActionSet_1.ActionSet();
        this.recognizers = new IntentRecognizerSet_1.IntentRecognizerSet();
        this.triggersAdded = false;
    }
    Library.prototype.localePath = function (path) {
        if (path) {
            this._localePath = path;
        }
        return this._localePath;
    };
    Library.prototype.recognize = function (session, callback) {
        this.recognizers.recognize(session.toRecognizeContext(), callback);
    };
    Library.prototype.recognizer = function (plugin) {
        this.recognizers.recognizer(plugin);
        return this;
    };
    Library.prototype.findRoutes = function (session, callback) {
        var _this = this;
        if (!this.triggersAdded) {
            this.forEachDialog(function (dialog, id) { return dialog.addDialogTrigger(_this.actions, id); });
            this.triggersAdded = true;
        }
        if (this._onFindRoutes) {
            this._onFindRoutes(session, callback);
        }
        else {
            this.defaultFindRoutes(session, callback);
        }
    };
    Library.prototype.onFindRoutes = function (handler) {
        this._onFindRoutes = handler;
    };
    Library.prototype.selectRoute = function (session, route) {
        if (this._onSelectRoute) {
            this._onSelectRoute(session, route);
        }
        else {
            this.defaultSelectRoute(session, route);
        }
    };
    Library.prototype.onSelectRoute = function (handler) {
        this._onSelectRoute = handler;
    };
    Library.prototype.findActiveDialogRoutes = function (session, topIntent, callback, dialogStack) {
        var _this = this;
        if (!dialogStack) {
            dialogStack = session.dialogStack();
        }
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var entry = Session_1.Session.activeDialogStackEntry(dialogStack);
        var parts = entry ? entry.id.split(':') : null;
        if (parts && parts[0] == this.name) {
            var dialog = this.dialog(parts[1]);
            if (dialog) {
                var context = session.toRecognizeContext();
                context.intent = topIntent;
                context.dialogData = entry.state;
                context.activeDialog = true;
                dialog.recognize(context, function (err, result) {
                    if (!err) {
                        if (result.score < 0.1) {
                            result.score = 0.1;
                        }
                        callback(null, Library.addRouteResult({
                            score: result.score,
                            libraryName: _this.name,
                            label: 'active_dialog_label',
                            routeType: Library.RouteTypes.ActiveDialog,
                            routeData: result
                        }, results));
                    }
                    else {
                        callback(err, null);
                    }
                });
            }
            else {
                logger.warn(session, "Active dialog '%s' not found in library.", entry.id);
                callback(null, results);
            }
        }
        else {
            callback(null, results);
        }
    };
    Library.prototype.selectActiveDialogRoute = function (session, route, newStack) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.ActiveDialog) {
            throw new Error('Invalid route type passed to Library.selectActiveDialogRoute().');
        }
        if (newStack) {
            session.dialogStack(newStack);
        }
        session.routeToActiveDialog(route.routeData);
    };
    Library.prototype.findStackActionRoutes = function (session, topIntent, callback, dialogStack) {
        var _this = this;
        if (!dialogStack) {
            dialogStack = session.dialogStack();
        }
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var context = session.toRecognizeContext();
        context.intent = topIntent;
        context.libraryName = this.name;
        context.routeType = Library.RouteTypes.StackAction;
        async.forEachOf((dialogStack || []).reverse(), function (entry, index, next) {
            var parts = entry.id.split(':');
            if (parts[0] == _this.name) {
                var dialog = _this.dialog(parts[1]);
                if (dialog) {
                    dialog.findActionRoutes(context, function (err, ra) {
                        if (!err) {
                            for (var i = 0; i < ra.length; i++) {
                                var r = ra[i];
                                if (r.routeData) {
                                    r.routeData.dialogId = entry.id;
                                    r.routeData.dialogIndex = index;
                                }
                                results = Library.addRouteResult(r, results);
                            }
                        }
                        next(err);
                    });
                }
                else {
                    logger.warn(session, "Dialog '%s' not found in library.", entry.id);
                    next(null);
                }
            }
            else {
                next(null);
            }
        }, function (err) {
            if (!err) {
                callback(null, results);
            }
            else {
                callback(err, null);
            }
        });
    };
    Library.prototype.selectStackActionRoute = function (session, route, newStack) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.StackAction) {
            throw new Error('Invalid route type passed to Library.selectStackActionRoute().');
        }
        if (newStack) {
            session.dialogStack(newStack);
        }
        var routeData = route.routeData;
        var parts = routeData.dialogId.split(':');
        this.dialog(parts[1]).selectActionRoute(session, route);
    };
    Library.prototype.findGlobalActionRoutes = function (session, topIntent, callback) {
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var context = session.toRecognizeContext();
        context.intent = topIntent;
        context.libraryName = this.name;
        context.routeType = Library.RouteTypes.GlobalAction;
        this.actions.findActionRoutes(context, function (err, ra) {
            if (!err) {
                for (var i = 0; i < ra.length; i++) {
                    var r = ra[i];
                    results = Library.addRouteResult(r, results);
                }
                callback(null, results);
            }
            else {
                callback(err, null);
            }
        });
    };
    Library.prototype.selectGlobalActionRoute = function (session, route) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.GlobalAction) {
            throw new Error('Invalid route type passed to Library.selectGlobalActionRoute().');
        }
        this.actions.selectActionRoute(session, route);
    };
    Library.prototype.defaultFindRoutes = function (session, callback) {
        var _this = this;
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        this.recognize(session, function (err, topIntent) {
            if (!err) {
                async.parallel([
                    function (cb) {
                        _this.findActiveDialogRoutes(session, topIntent, function (err, routes) {
                            if (!err && routes) {
                                routes.forEach(function (r) { return results = Library.addRouteResult(r, results); });
                            }
                            cb(err);
                        });
                    },
                    function (cb) {
                        _this.findStackActionRoutes(session, topIntent, function (err, routes) {
                            if (!err && routes) {
                                routes.forEach(function (r) { return results = Library.addRouteResult(r, results); });
                            }
                            cb(err);
                        });
                    },
                    function (cb) {
                        _this.findGlobalActionRoutes(session, topIntent, function (err, routes) {
                            if (!err && routes) {
                                routes.forEach(function (r) { return results = Library.addRouteResult(r, results); });
                            }
                            cb(err);
                        });
                    }
                ], function (err) {
                    if (!err) {
                        callback(null, results);
                    }
                    else {
                        callback(err, null);
                    }
                });
            }
            else {
                callback(err, null);
            }
        });
    };
    Library.prototype.defaultSelectRoute = function (session, route) {
        switch (route.routeType || '') {
            case Library.RouteTypes.ActiveDialog:
                this.selectActiveDialogRoute(session, route);
                break;
            case Library.RouteTypes.StackAction:
                this.selectStackActionRoute(session, route);
                break;
            case Library.RouteTypes.GlobalAction:
                this.selectGlobalActionRoute(session, route);
                break;
            default:
                throw new Error('Invalid route type passed to Library.selectRoute().');
        }
    };
    Library.addRouteResult = function (route, current) {
        if (!current || current.length < 1 || route.score > current[0].score) {
            current = [route];
        }
        else if (route.score == current[0].score) {
            current.push(route);
        }
        return current;
    };
    Library.bestRouteResult = function (routes, dialogStack, rootLibraryName) {
        var bestLibrary = rootLibraryName;
        if (dialogStack) {
            dialogStack.forEach(function (entry) {
                var parts = entry.id.split(':');
                for (var i = 0; i < routes.length; i++) {
                    if (routes[i].libraryName == parts[0]) {
                        bestLibrary = parts[0];
                        break;
                    }
                }
            });
        }
        var best;
        var bestPriority = 5;
        for (var i = 0; i < routes.length; i++) {
            var r = routes[i];
            if (r.score > 0.0) {
                var priority;
                switch (r.routeType) {
                    default:
                        priority = 1;
                        break;
                    case Library.RouteTypes.ActiveDialog:
                        priority = 2;
                        break;
                    case Library.RouteTypes.StackAction:
                        priority = 3;
                        break;
                    case Library.RouteTypes.GlobalAction:
                        priority = 4;
                        break;
                }
                if (priority < bestPriority) {
                    best = r;
                    bestPriority = priority;
                }
                else if (priority == bestPriority) {
                    switch (priority) {
                        case 3:
                            if (r.routeData.dialogIndex > best.routeData.dialogIndex) {
                                best = r;
                            }
                            break;
                        case 4:
                            if (bestLibrary && best.libraryName !== bestLibrary && r.libraryName == bestLibrary) {
                                best = r;
                            }
                            break;
                    }
                }
            }
        }
        return best;
    };
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
                d = new SimpleDialog_1.SimpleDialog(SimpleDialog_1.createWaterfall(dialog));
            }
            else if (typeof dialog == 'function') {
                d = new SimpleDialog_1.SimpleDialog(SimpleDialog_1.createWaterfall([dialog]));
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
    Library.prototype.findDialog = function (libName, dialogId) {
        var d;
        var lib = this.library(libName);
        if (lib) {
            d = lib.dialog(dialogId);
        }
        return d;
    };
    Library.prototype.forEachDialog = function (callback) {
        for (var id in this.dialogs) {
            callback(this.dialog(id), id);
        }
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
    Library.prototype.forEachLibrary = function (callback) {
        for (var lib in this.libraries) {
            callback(this.libraries[lib]);
        }
    };
    Library.prototype.libraryList = function (reverse) {
        if (reverse === void 0) { reverse = false; }
        var list = [];
        var added = {};
        function addChildren(lib) {
            if (!added.hasOwnProperty(lib.name)) {
                added[lib.name] = true;
                if (!reverse) {
                    list.push(lib);
                }
                lib.forEachLibrary(function (child) { return addChildren(child); });
                if (reverse) {
                    list.push(lib);
                }
            }
        }
        addChildren(this);
        return list;
    };
    Library.prototype.beginDialogAction = function (name, id, options) {
        this.actions.beginDialogAction(name, id, options);
        return this;
    };
    Library.prototype.endConversationAction = function (name, msg, options) {
        this.actions.endConversationAction(name, msg, options);
        return this;
    };
    Library.RouteTypes = {
        GlobalAction: 'GlobalAction',
        StackAction: 'StackAction',
        ActiveDialog: 'ActiveDialog'
    };
    return Library;
}(events_1.EventEmitter));
exports.Library = Library;
exports.systemLib = new Library(consts.Library.system);
exports.systemLib.localePath(path.join(__dirname, '../locale/'));
