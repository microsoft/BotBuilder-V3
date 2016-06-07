var msg = require('../Message');
var Fact = (function () {
    function Fact(session) {
        this.session = session;
        this.data = { value: '' };
    }
    Fact.prototype.key = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.key = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    Fact.prototype.value = function (v) {
        this.data.value = v || '';
        return this;
    };
    Fact.prototype.toFact = function () {
        return this.data;
    };
    return Fact;
})();
exports.Fact = Fact;
