"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const WaterfallDialog_1 = require("../dialogs/WaterfallDialog");
const ActionSet_1 = require("../dialogs/ActionSet");
const IntentRecognizerSet_1 = require("../dialogs/IntentRecognizerSet");
const Session_1 = require("../Session");
const consts = require("../consts");
const utils = require("../utils");
const events_1 = require("events");
const path = require("path");
const async = require("async");
class Library extends events_1.EventEmitter {
    constructor(name) {
        super();
        this.name = name;
        this.dialogs = {};
        this.libraries = {};
        this.actions = new ActionSet_1.ActionSet();
        this.recognizers = new IntentRecognizerSet_1.IntentRecognizerSet();
        this.triggersAdded = false;
    }
    clone(copyTo, newName) {
        var obj = copyTo || new Library(newName || this.name);
        for (var id in this.dialogs) {
            obj.dialogs[id] = this.dialogs[id];
        }
        for (var name in this.libraries) {
            obj.libraries[name] = this.libraries[name];
        }
        this.actions.clone(obj.actions);
        this.recognizers.clone(obj.recognizers);
        obj._localePath = this._localePath;
        obj._onFindRoutes = this._onFindRoutes;
        obj._onSelectRoute = this._onSelectRoute;
        obj.triggersAdded = this.triggersAdded;
        return obj;
    }
    localePath(path) {
        if (path) {
            this._localePath = path;
        }
        return this._localePath;
    }
    recognize(context, callback) {
        if (this.recognizers.length > 0 && context.libraryName !== this.name) {
            this.recognizers.recognize(context, (err, result) => {
                if (result && result.score > 0) {
                    context.logger.log(null, this.logPrefix() + 'recognize() recognized: ' +
                        result.intent + '(' + result.score + ')');
                }
                callback(err, result);
            });
        }
        else {
            callback(null, context.intent || { intent: 'None', score: 0.0 });
        }
    }
    recognizer(plugin) {
        this.recognizers.recognizer(plugin);
        return this;
    }
    findRoutes(context, callback) {
        if (!this.triggersAdded) {
            this.forEachDialog((dialog, id) => dialog.addDialogTrigger(this.actions, this.name + ':' + id));
            this.triggersAdded = true;
        }
        if (this._onFindRoutes) {
            this._onFindRoutes(context, callback);
        }
        else {
            this.defaultFindRoutes(context, callback);
        }
    }
    onFindRoutes(handler) {
        this._onFindRoutes = handler;
    }
    selectRoute(session, route) {
        if (this._onSelectRoute) {
            this._onSelectRoute(session, route);
        }
        else {
            this.defaultSelectRoute(session, route);
        }
    }
    onSelectRoute(handler) {
        this._onSelectRoute = handler;
    }
    findActiveDialogRoutes(context, callback, dialogStack) {
        if (!dialogStack) {
            dialogStack = context.dialogStack();
        }
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var entry = Session_1.Session.activeDialogStackEntry(dialogStack);
        var parts = entry ? entry.id.split(':') : null;
        if (parts && parts[0] == this.name) {
            var dialog = this.dialog(parts[1]);
            if (dialog) {
                var ctx = utils.clone(context);
                ctx.libraryName = this.name;
                ctx.dialogData = entry.state;
                ctx.activeDialog = true;
                dialog.recognize(ctx, (err, result) => {
                    if (!err) {
                        if (result.score < 0.1) {
                            result.score = 0.1;
                        }
                        callback(null, Library.addRouteResult({
                            score: result.score,
                            libraryName: this.name,
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
                context.logger.warn(context.dialogStack(), "Active dialog '" + entry.id + "' not found in library.");
                callback(null, results);
            }
        }
        else {
            callback(null, results);
        }
    }
    selectActiveDialogRoute(session, route, newStack) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.ActiveDialog) {
            throw new Error('Invalid route type passed to Library.selectActiveDialogRoute().');
        }
        if (newStack) {
            session.dialogStack(newStack);
        }
        session.routeToActiveDialog(route.routeData);
    }
    findStackActionRoutes(context, callback, dialogStack) {
        if (!dialogStack) {
            dialogStack = context.dialogStack();
        }
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var ctx = utils.clone(context);
        ctx.libraryName = this.name;
        ctx.routeType = Library.RouteTypes.StackAction;
        async.forEachOf(dialogStack || [], (entry, index, next) => {
            var parts = entry.id.split(':');
            if (parts[0] == this.name) {
                var dialog = this.dialog(parts[1]);
                if (dialog) {
                    dialog.findActionRoutes(ctx, (err, ra) => {
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
                    ctx.logger.warn(ctx.dialogStack(), "Dialog '" + entry.id + "' not found in library.");
                    next(null);
                }
            }
            else {
                next(null);
            }
        }, (err) => {
            if (!err) {
                callback(null, results);
            }
            else {
                callback(err, null);
            }
        });
    }
    selectStackActionRoute(session, route, newStack) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.StackAction) {
            throw new Error('Invalid route type passed to Library.selectStackActionRoute().');
        }
        if (newStack) {
            session.dialogStack(newStack);
        }
        var routeData = route.routeData;
        var parts = routeData.dialogId.split(':');
        this.dialog(parts[1]).selectActionRoute(session, route);
    }
    findGlobalActionRoutes(context, callback) {
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        var ctx = utils.clone(context);
        ctx.libraryName = this.name;
        ctx.routeType = Library.RouteTypes.GlobalAction;
        this.actions.findActionRoutes(ctx, (err, ra) => {
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
    }
    selectGlobalActionRoute(session, route) {
        if (!route || route.libraryName !== this.name || route.routeType !== Library.RouteTypes.GlobalAction) {
            throw new Error('Invalid route type passed to Library.selectGlobalActionRoute().');
        }
        this.actions.selectActionRoute(session, route);
    }
    defaultFindRoutes(context, callback) {
        var explain = '';
        var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
        this.recognize(context, (err, topIntent) => {
            if (!err) {
                var ctx = utils.clone(context);
                ctx.intent = topIntent && topIntent.score > 0 ? topIntent : null;
                ctx.libraryName = this.name;
                async.parallel([
                    (cb) => {
                        this.findActiveDialogRoutes(ctx, (err, routes) => {
                            if (!err && routes) {
                                routes.forEach((r) => {
                                    results = Library.addRouteResult(r, results);
                                    if (r.score > 0) {
                                        explain += '\n\tActiveDialog(' + r.score + ')';
                                    }
                                });
                            }
                            cb(err);
                        });
                    },
                    (cb) => {
                        this.findStackActionRoutes(ctx, (err, routes) => {
                            if (!err && routes) {
                                routes.forEach((r) => {
                                    results = Library.addRouteResult(r, results);
                                    if (r.score > 0) {
                                        explain += '\n\tStackAction(' + r.score + ')';
                                    }
                                });
                            }
                            cb(err);
                        });
                    },
                    (cb) => {
                        this.findGlobalActionRoutes(ctx, (err, routes) => {
                            if (!err && routes) {
                                routes.forEach((r) => {
                                    results = Library.addRouteResult(r, results);
                                    if (r.score > 0) {
                                        explain += '\n\tGlobalAction(' + r.score + ')';
                                    }
                                });
                            }
                            cb(err);
                        });
                    }
                ], (err) => {
                    if (!err) {
                        if (explain.length > 0) {
                            context.logger.log(null, this.logPrefix() + '.findRoutes() explanation:' + explain);
                        }
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
    }
    defaultSelectRoute(session, route) {
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
    }
    static addRouteResult(route, current) {
        if (!current || current.length < 1 || route.score > current[0].score) {
            current = [route];
        }
        else if (route.score == current[0].score) {
            current.push(route);
        }
        return current;
    }
    static bestRouteResult(routes, dialogStack, rootLibraryName) {
        var bestLibrary = rootLibraryName;
        if (dialogStack) {
            dialogStack.forEach((entry) => {
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
    }
    dialog(id, dialog, replace) {
        var d;
        if (dialog) {
            if (id.indexOf(':') >= 0) {
                id = id.split(':')[1];
            }
            if (this.dialogs.hasOwnProperty(id) && !replace) {
                throw new Error("Dialog[" + id + "] already exists in library[" + this.name + "].");
            }
            if (Array.isArray(dialog) || typeof dialog === 'function') {
                d = new WaterfallDialog_1.WaterfallDialog(dialog);
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
    }
    findDialog(libName, dialogId) {
        var d;
        var lib = this.library(libName);
        if (lib) {
            d = lib.dialog(dialogId);
        }
        return d;
    }
    forEachDialog(callback) {
        for (var id in this.dialogs) {
            callback(this.dialog(id), id);
        }
    }
    library(lib) {
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
    }
    forEachLibrary(callback) {
        for (var lib in this.libraries) {
            callback(this.libraries[lib]);
        }
    }
    libraryList(reverse = false) {
        var list = [];
        var added = {};
        function addChildren(lib) {
            if (!added.hasOwnProperty(lib.name)) {
                added[lib.name] = true;
                if (!reverse) {
                    list.push(lib);
                }
                lib.forEachLibrary((child) => addChildren(child));
                if (reverse) {
                    list.push(lib);
                }
            }
        }
        addChildren(this);
        return list;
    }
    beginDialogAction(name, id, options) {
        this.actions.beginDialogAction(name, id, options);
        return this;
    }
    endConversationAction(name, msg, options) {
        this.actions.endConversationAction(name, msg, options);
        return this;
    }
    customAction(options) {
        this.actions.customAction(options);
        return this;
    }
    logPrefix() {
        return 'Library("' + this.name + '")';
    }
}
Library.RouteTypes = {
    GlobalAction: 'GlobalAction',
    StackAction: 'StackAction',
    ActiveDialog: 'ActiveDialog'
};
exports.Library = Library;
exports.systemLib = new Library(consts.Library.system);
exports.systemLib.localePath(path.join(__dirname, '../locale/'));
