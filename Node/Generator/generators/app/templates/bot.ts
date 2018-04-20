/* ----------------------------------------------------------------
*   Bot created with botbuilder Yeoman Generator
*   https://github.com/microsoftdx/generator-botbuilder
*
*   All default dialogs are located in ./dialogs
*   You can add additional dialogs below as needed
---------------------------------------------------------------- */

import * as builder from 'botbuilder';
import dialog from './dialogs/<%= defaultDialog %>';

const bot = new builder.UniversalBot(
    new builder.ChatConnector({
        appId: process.env.MICROSOFT_APP_ID,
        appPassword: process.env.MICROSOFT_APP_PASSWORD
    }),
    dialog.waterfall
);
<%= luisRegistration %>
export default bot;
