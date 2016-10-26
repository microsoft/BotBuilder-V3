"use strict";
var uuid = require('node-uuid');
var RejectAction = (function () {
    function RejectAction(session) {
        this.session = session;
        this.data = {};
        this.data.action = 'reject';
        this.data.operationId = uuid.v4();
    }
    RejectAction.prototype.toAction = function () {
        return this.data;
    };
    return RejectAction;
}());
exports.RejectAction = RejectAction;
