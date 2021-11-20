
# BotBuilder SDK Build Process Documentation

To verify the consistency and quality of the code, we configured a Build CI process for each repository that belongs to the BotBuilder SDK. The main idea behind this practice is to test the code frequently as the development process proceeds, so that possible issues are identified and rectified early.

The next table shows a comparison of the Build CI process between the BotBuilder SDK repositories.  

<table>
    <tr>
        <th align="center" colspan="2">CI Pipeline</th>
        <th align="center">BotBuilder-DotNet</th>
        <th align="center">BotBuilder-JS</th>
        <th align="center">BotBuilder-Java</th>
        <th align="center">BotBuilder-Python</th>
    </tr>
    <tr align="center">
        <td colspan="2">License</td>
        <td>✔️</td>
        <td>✔️</td>
        <td>✔️</td>
        <td>✔️</td>
    </tr>
    <tr align="center">
        <td colspan="2">Coverage</td>
        <td>✔️</td>
        <td>✔️</td>
        <td>✔️</td>
        <td>✔️</td>
    </tr>
    <tr align="center">
        <td rowspan="2">Build CI</td>
        <td>Windows</td>
        <td>✔️</td>
        <td>✔️</td>
        <td>✔️</td>
        <td></td>
    </tr>
    <tr align="center">
        <td>Mac/Linux</td>
        <td>✔️</td>
        <td></td>
        <td></td>
        <td>✔️</td>
    </tr>
    <tr align="center">
        <td rowspan="2">Nightly Functional Test</td>
        <td>Windows</td>
        <td>✔️</td>
        <td>✔️</td>
        <td></td>
        <td></td>
    </tr>
    <tr align="center">
        <td>Linux</td>
        <td>✔️</td>
        <td>✔️</td>
        <td></td>
        <td></td>
    </tr>
    <tr align="center">
        <td rowspan="2">Browser Compatibility</td>
        <td>Chrome</td>
        <td></td>
        <td>✔️</td>
        <td></td>
        <td></td>
    </tr>
    <tr align="center">
        <td>Firefox</td>
        <td></td>
        <td>✔️</td>
        <td></td>
        <td></td>
    </tr>
    <tr align="center">
        <td rowspan="2">Misc</td>
        <td>Functional Test Bot Image</td>
        <td></td>
        <td></td>
        <td></td>
        <td>✔️</td>
    </tr>
    <tr align="center">
        <td>Functional Test Bot Container</td>
        <td></td>
        <td></td>
        <td></td>
        <td>✔️</td>
    </tr>
</table>



## **Pipelines details**

### **Build CI**

This pipeline aims to verify the build and tests configured of the repository. It consists of building and running the tests using a specific OS, which can be Windows or Linux.

Each repository has its custom configuration depending on the language used to build.

**DotNet**

- NetCore 2.1.x and 3.1.x

- Binary compatibility tool

> Running on Windows and Mac OS.

**Javascript**

- Node 13

- NPM

> Running on windows only

**Java**

- Java 1.8

- Maven 

> Running on Azure and Travis CI

**Python** 

Python 3.7.6

> Running on Linux only

**Configuration**

This pipeline will run every pull request pointing to the master branch.

**Possible build failure causes:**

- Error in the compile process
- Error installing project dependencies
- Unit Tests failing result

### **Nightly Functional Test**

This pipeline aims to verify the interaction of a bot deployed in Azure using a specific OS, which can be Windows or Linux. It consists of building, deploying and running some tests against the bot Azure website endpoint. 

**Configuration**

This pipeline will run every night 

**Possible build failure causes:**

- Error in the compile process
- Error installing project dependencies
- Deployment process not completed
- Environment specific errors
- Bot functionality errors

### **Browser Compatibility**

This pipeline aims to verify the browser compatibility of the BotBuilder-JS repository. It consists of building and deploying a browser-echo bot. Then, it runs some UI against the bot Azure website endpoint. 

**Configuration**

This pipeline will run every pull request pointing to the master branch.

**Possible build failure causes:**

- Error in the compile process
- Error installing project dependencies
- Deployment process not completed
- Environment specific errors
- Bot functionality errors
- Webpack configuration issues
- Browser compatibility comprised

### **Adapters**

This set of pipelines aims to verify the correct integration of the adapter and the bot. 

It consists of building and deploying an echo bot configured with the selected adapter. Then, it runs some tests against the communication app of the adapter. 

Each adapter has its bot and test suite.

**Configuration**

This pipelines will run every night

**Possible build failure causes:**

- Error in the compile process
- Error installing project dependencies
- Deployment process not completed
- Environment specific errors
- Bot functionality errors
- Webpack configuration issues
- Browser compatibility comprised

### **Miscellaneous**

This category contains the extra pipelines configured for specific repositories.

**BotBuilder-Python**

This pipeline aims to verify the correct deployment of a bot created using BotBuilder-Python. It consists of deploying an echo bot using the bot image and container approaches. 

**Configuration**

This pipeline will run every pull request pointing to the master branch.

**Possible build failure causes:**

- Error installing project dependencies
- Deployment process not completed
- Environment specific errors
- Bot functionality errors
- Unit Tests failing result
