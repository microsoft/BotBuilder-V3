# Tips on trying out the sample

To try out this sample project thoroughly, you need to deploy the bot on your server, and set up the configurations, including `Microsoft.Bot.Builder.Calling.CallbackUrl` in `appsettings.json`. You need to replace `localhost:3999` with the actual host name of your server. SSL is mandatory.

If you are not in LAN network, you can try tunneling your local service via `ngrok`.

You need to register a bot on https://dev.botframework.com , set up the Skype channel, and enable Calling / IVR audio calls. You need to fill the webhook URL that corresponds to `calling/call` web API method, i.e. something in the form of `https://company.org/api/calling/call`.

Add the bot as your Skype contact, wait for a moment (a couple of minutes) until you can make audio calls to the bot in Skype, and then just do it XD

If you are deploying the sample on a Linux machine, the [Linux remote debugging](https://blogs.msdn.microsoft.com/devops/2017/01/26/debugging-net-core-on-unix-over-ssh/) functionality provided by VS IDE is really, really handy.
