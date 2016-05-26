function preferButtons(session, choiceCnt, rePrompt) {
    switch (getChannelId(session)) {
        case 'facebook':
            return (choiceCnt <= 3);
        case 'telegram':
        case 'kik':
            return true;
        default:
            return false;
    }
}
exports.preferButtons = preferButtons;
function getChannelId(session) {
    var account = session.message.from || session.message.to;
    return account.channelId.toLowerCase();
}
exports.getChannelId = getChannelId;
