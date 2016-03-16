var SESSION_STATE_KEY = 'B9047F2ED6C4_SESSION_STATE';
var CookieStore = (function () {
    /** Wraps a cookie store around a Request+Response. Stored cookies expire after 3 hours by default. */
    function CookieStore(req, res, maxAge) {
        if (maxAge === void 0) { maxAge = 10800000; }
        this.req = req;
        this.res = res;
        this.maxAge = maxAge;
    }
    CookieStore.prototype.load = function (id, cb) {
        try {
            var s = this.req.cookies[SESSION_STATE_KEY];
            var state = (typeof s == 'string' ? JSON.parse(s) : null);
            cb(null, state);
        }
        catch (err) {
            cb(err, null);
        }
    };
    CookieStore.prototype.save = function (id, state, cb) {
        try {
            if (state) {
                this.res.cookie(SESSION_STATE_KEY, JSON.stringify(state), { maxAge: this.maxAge });
            }
            else {
                this.res.clearCookie(id);
            }
            cb(null);
        }
        catch (err) {
            cb(err);
        }
    };
    return CookieStore;
})();
exports.CookieStore = CookieStore;
var MemoryStore = (function () {
    function MemoryStore(state) {
        if (state === void 0) { state = {}; }
        this.state = state;
    }
    MemoryStore.prototype.load = function (id, cb) {
        try {
            var state = clone(this.state[id]);
            cb(null, state);
        }
        catch (err) {
            cb(err, null);
        }
    };
    MemoryStore.prototype.save = function (id, state, cb) {
        try {
            if (state) {
                this.state[id] = clone(state);
            }
            else if (this.state.hasOwnProperty(id)) {
                delete this.state[id];
            }
            if (cb) {
                cb(null);
            }
        }
        catch (err) {
            if (cb) {
                cb(err);
            }
        }
    };
    return MemoryStore;
})();
exports.MemoryStore = MemoryStore;
function clone(obj) {
    var cpy = {};
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            cpy[key] = obj[key];
        }
    }
    return cpy;
}
//# sourceMappingURL=SessionStore.js.map