"use strict";
var utils = require('../utils');
exports.VoiceGender = {
    male: 'male',
    female: 'female'
};
exports.SayAs = {
    yearMonthDay: 'yearMonthDay',
    monthDayYear: 'monthDayYear',
    dayMonthYear: 'dayMonthYear',
    yearMonth: 'yearMonth',
    monthYear: 'monthYear',
    monthDay: 'monthDay',
    dayMonth: 'dayMonth',
    day: 'day',
    month: 'month',
    year: 'year',
    cardinal: 'cardinal',
    ordinal: 'ordinal',
    letters: 'letters',
    time12: 'time12',
    time24: 'time24',
    telephone: 'telephone',
    name: 'name',
    phoneticName: 'phoneticName'
};
var Prompt = (function () {
    function Prompt(session) {
        this.session = session;
        this.data = session ? utils.clone(session.promptDefaults || {}) : {};
    }
    Prompt.prototype.value = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.value = utils.fmtText(this.session, text, args);
        }
        else {
            this.data.value = null;
        }
        return this;
    };
    Prompt.prototype.fileUri = function (uri) {
        if (uri) {
            this.data.fileUri = uri;
        }
        return this;
    };
    Prompt.prototype.voice = function (gender) {
        if (gender) {
            this.data.voice = gender;
        }
        return this;
    };
    Prompt.prototype.culture = function (locale) {
        if (locale) {
            this.data.culture = locale;
        }
        return this;
    };
    Prompt.prototype.silenceLengthInMilliseconds = function (time) {
        if (time) {
            this.data.silenceLengthInMilliseconds = time;
        }
        return this;
    };
    Prompt.prototype.emphasize = function (flag) {
        this.data.emphasize = flag;
        return this;
    };
    Prompt.prototype.sayAs = function (type) {
        if (type) {
            this.data.sayAs = type;
        }
        return this;
    };
    Prompt.prototype.toPrompt = function () {
        return this.data;
    };
    Prompt.text = function (session, text) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        args.unshift(text);
        var prompt = new Prompt(session);
        Prompt.prototype.value.apply(prompt, args);
        return prompt;
    };
    Prompt.file = function (session, uri) {
        return new Prompt(session).fileUri(uri);
    };
    Prompt.silence = function (session, time) {
        return new Prompt(session).value(null).silenceLengthInMilliseconds(time);
    };
    return Prompt;
}());
exports.Prompt = Prompt;
