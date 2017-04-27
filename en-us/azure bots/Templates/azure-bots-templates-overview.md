---
layout: 'redirect'
permalink: /en-us/azure-bot-service/templates/overview/
redir_to: 'https://docs.microsoft.com/en-us/bot-framework/azure/azure-bot-service-templates'
sitemap: false
---

Azure Bot Service is powered by the serveless infrastructure of Azure Functions, and it shares its [runtime concepts](https://azure.microsoft.com/en-us/documentation/articles/functions-reference/){:target="_blank"}, which you should become familiar with.

All Azure Bot Service bots include the following files:


<div id="thetabs1">
    <ul>
        <li data-lang="csharp"><a href="#tab11">C#</a></li>
        <li data-lang="node"><a href="#tab12">Node.js</a></li>
    </ul>

    <div id="tab11">

    <table>
      <tr>
        <th><strong>File</strong></th>
        <th><strong>Description</strong></th>
      </tr>
      <tr>
        <td>Bot.sln</td>
        <td>The Microsoft Visual Studio solutions file. Used locally if you set up <a href="/en-us/azure-bot-service/manage/setting-up-continuous-integration/">continuous integration</a>.</td>
      </tr>
      <tr>
        <td>commands.json</td>
        <td>This file contains the commands that start debughost in Task Runner Explorer when you open the Bot.sln file. If you don't install Task Runner Explorer, you can delete this file.</td>
      </tr>
      <tr>
        <td>debughost.cmd</td>
        <td>This file contains the commands to load and run your bot. Used locally if you set up continuous integration and want to debug your bot locally. For more information, see <a href="/en-us/azure-bot-service/manage/debug/#debugging-c-bots-built-using-the-azure-bot-service-on-windows">Debugging C# bots built using the Azure Bot Service on Windows</a>. The file also contains your app ID and password. You would set the ID and password if you want to debug authentication. If you set these, you must provide the ID and password in the emulator, too.</td>
      </tr>
      <tr>
        <td>function.json</td>
        <td>This file contains the function’s bindings. You should not modify this file.</td>
      </tr>
      <tr>
        <td>host.json</td>
        <td>A metadata file that contains the global configuration options affecting the function.</td>
      </tr>
      <tr>
        <td>project.json</td>
        <td>This file contains the project’s NuGet references. You should only have to change this file if you add a new reference.</td>
      </tr>
      <tr>
        <td>project.lock.json</td>
        <td>This file is generated automatically, and should not be modified.</td>
      </tr>
      <tr>
        <td>readme.md</td>
        <td>This file contains notes that you should read before using or modifying the bot.</td>
      </tr>
    </table>

    </div>
    <div id="tab12">

    <table>
      <tr>
        <th><strong>File</strong></th>
        <th><strong>Description</strong></th>
      </tr>
      <tr>
        <td>function.json</td>
        <td>This file contains the function’s bindings. You should not modify this file.</td>
      </tr>
      <tr>
        <td>host.json</td>
        <td>A metadata file that contains the global configuration options affecting the function.</td>
      </tr>
      <tr>
        <td>package.json</td>
        <td>This file contains the project’s NuGet references. You should only have to change this file if you add a new reference.</td>
      </tr>
    </table>


    </div>  
</div>

