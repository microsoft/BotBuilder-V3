"use strict";
var uuid = require('node-uuid');
var AnswerAction = (function () {
    function AnswerAction(session) {
        this.session = session;
        this.data = {};
        this.data.action = 'answer';
        this.data.operationId = uuid.v4();
    }
    AnswerAction.prototype.acceptModalityTypes = function (types) {
        if (types) {
            this.data.acceptModalityTypes = types;
        }
        return this;
    };
    AnswerAction.prototype.toAction = function () {
        return this.data;
    };
    return AnswerAction;
}());
exports.AnswerAction = AnswerAction;
