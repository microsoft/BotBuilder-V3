"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ClaimsIdentity = (function () {
    function ClaimsIdentity(claims, isAuthenticated) {
        this.claims = claims;
        this.isAuthenticated = isAuthenticated;
    }
    ClaimsIdentity.prototype.getClaimValue = function (claimType) {
        var claim = this.claims.find(function (c) { return c.type === claimType; });
        return claim ? claim.value : null;
    };
    return ClaimsIdentity;
}());
exports.ClaimsIdentity = ClaimsIdentity;
