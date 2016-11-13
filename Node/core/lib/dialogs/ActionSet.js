"use strict";
var utils = require('../utils');
var async = require('async');
var ActionSet = (function () {
    function ActionSet() {
        this.actions = {};
    }
    ActionSet.prototype.addDialogTrigger = function (actions, dialogId) {
        if (this.trigger) {
            actions.beginDialogAction(dialogId, dialogId, this.trigger);
        }
    };
    ActionSet.prototype.findActionRoutes = function (context, callback) {
        var results = [{ score: 0.0, libraryName: context.libraryName }];
        function addRoute(route) {
            if (route.score > 0 && route.routeData) {
                route.routeData.libraryName = context.libraryName;
                if (route.score > results[0].score) {
                    results = [route];
                }
                else if (route.score == results[0].score) {
                    results.push(route);
                }
            }
        }
        function matchExpression(action, entry, cb) {
            if (entry.options.matches) {
                var bestScore = 0.0;
                var routeData;
                var matches = Array.isArray(entry.options.matches) ? entry.options.matches : [entry.options.matches];
                matches.forEach(function (exp) {
                    if (typeof exp == 'string') {
                        if (context.intent && exp === context.intent.intent && context.intent.score > bestScore) {
                            bestScore = context.intent.score;
                            routeData = {
                                action: action,
                                intent: context.intent
                            };
                        }
                    }
                    else {
                        var matches = exp.exec(text);
                        if (matches && matches.length) {
                            var intent = {
                                score: matches[0].length / text.length,
                                intent: exp.toString(),
                                expression: exp,
                                matched: matches
                            };
                            if (intent.score > bestScore) {
                                bestScore = intent.score;
                                routeData = {
                                    action: action,
                                    intent: intent
                                };
                            }
                        }
                    }
                });
                var intentThreshold = entry.options.intentThreshold || 0.1;
                if (bestScore >= intentThreshold) {
                    cb(null, bestScore, routeData);
                }
                else {
                    cb(null, 0.0, null);
                }
            }
            else {
                cb(null, 0.0, null);
            }
        }
        var text = context.message.text || '';
        if (text.indexOf('action?') == 0) {
            var parts = text.split('?')[1].split('=');
            var name = parts[0];
            if (this.actions.hasOwnProperty(name)) {
                var options = this.actions[name].options;
                var routeData = { action: name };
                if (parts.length > 1) {
                    parts.shift();
                    routeData.data = parts.join('=');
                }
                addRoute({
                    score: 1.0,
                    libraryName: context.libraryName,
                    label: options.label || name,
                    routeType: context.routeType,
                    routeData: routeData
                });
            }
            callback(null, results);
        }
        else {
            async.forEachOf(this.actions, function (entry, action, cb) {
                if (entry.options.onFindAction) {
                    entry.options.onFindAction(context, function (err, score, routeData) {
                        if (!err) {
                            routeData = routeData || {};
                            routeData.action = action;
                            addRoute({
                                score: score,
                                libraryName: context.libraryName,
                                label: entry.options.label || action,
                                routeType: context.routeType,
                                routeData: routeData
                            });
                        }
                        cb(err);
                    });
                }
                else {
                    matchExpression(action, entry, function (err, score, routeData) {
                        if (!err && routeData) {
                            addRoute({
                                score: score,
                                libraryName: context.libraryName,
                                label: entry.options.label || name,
                                routeType: context.routeType,
                                routeData: routeData
                            });
                        }
                        cb(err);
                    });
                }
            }, function (err) {
                if (!err) {
                    callback(null, results);
                }
                else {
                    callback(err, null);
                }
            });
        }
    };
    ActionSet.prototype.selectActionRoute = function (session, route) {
        function next() {
            entry.handler(session, routeData);
        }
        var routeData = route.routeData;
        var entry = this.actions[routeData.action];
        if (entry.options.onSelectAction) {
            entry.options.onSelectAction(session, routeData, next);
        }
        else {
            next();
        }
    };
    ActionSet.prototype.cancelAction = function (name, msg, options) {
        return this.action(name, function (session, args) {
            if (args && typeof args.dialogIndex === 'number') {
                if (msg) {
                    session.send(msg);
                }
                session.cancelDialog(args.dialogIndex);
            }
        }, options);
    };
    ActionSet.prototype.reloadAction = function (name, msg, options) {
        if (options === void 0) { options = {}; }
        return this.action(name, function (session, args) {
            if (msg) {
                session.send(msg);
            }
            session.cancelDialog(args.dialogIndex, args.dialogId, options.dialogArgs);
        }, options);
    };
    ActionSet.prototype.beginDialogAction = function (name, id, options) {
        if (options === void 0) { options = {}; }
        return this.action(name, function (session, args) {
            if (options.dialogArgs) {
                utils.copyTo(options.dialogArgs, args);
            }
            if (id.indexOf(':') < 0) {
                var lib = args.dialogId ? args.dialogId.split(':')[0] : args.libraryName;
                id = lib + ':' + id;
            }
            session.beginDialog(id, args);
        }, options);
    };
    ActionSet.prototype.endConversationAction = function (name, msg, options) {
        return this.action(name, function (session, args) {
            session.endConversation(msg);
        }, options);
    };
    ActionSet.prototype.triggerAction = function (options) {
        this.trigger = options;
        return this;
    };
    ActionSet.prototype.action = function (name, handler, options) {
        if (options === void 0) { options = {}; }
        if (this.actions.hasOwnProperty(name)) {
            throw new Error("DialogAction[" + name + "] already exists.");
        }
        this.actions[name] = { handler: handler, options: options };
        return this;
    };
    return ActionSet;
}());
exports.ActionSet = ActionSet;
