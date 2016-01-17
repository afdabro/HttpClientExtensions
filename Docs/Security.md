#Security

Cloud security is a broad term that is often misrepresented. 
Yes, your data is in the cloud. A hacker *might* find it harder to retrieve your data simply because it is stored offsite. 
But cloud services still fall victim to the same low hanging fruit issues. Security is everyone’s responsibility. 
The [security development lifecycle (SDL)](https://www.microsoft.com/en-us/sdl/) should be coordinated within an [Agile development life cycle](http://www.ambysoft.com/essays/agileLifecycle.html). Design for Security. Build [threat models](https://msdn.microsoft.com/en-us/library/ff648644.aspx). 
Always put yourself in a security mindset; "If I was a hacker, how would I infiltrate this system?" 
Most all, familiarize yourself with known web vulnerabilities within your chosen technology stack. 
[OWASP top 10](https://www.owasp.org/index.php/Main_Page) is a good "general" starting point. 
Security information for Cloud N-tier and Micro-Service architectures are a little lacking.


##Crash Dumps

A cloud service could fail and generate a crash dump which is normally sent to a blob file storage. Securing passwords, tokens, etc. in memory is still *extremely* important. A hacker could (in-theory) gain access to the crush dump storage.
I have been on a few secure code reviews where a service's password is stored as a string. 
Use [`SecureString`](http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx) to store any sensitive configuration settings in memory.
Within your application's trust boundary, always try to pass a `SecureString` to constructors or as a parameter. Eventually, you might be forced to call a third-party api which only accepts a string but at least your application is slightly more secure.

##Security Tokens

At some point, your service will need to store a token ([JSON Web Token](https://jwt.io/introduction/), [Shared Access Signature Token](https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/), etc.) to authenticate with another service. This token will eventually expire and might contain claims. 
Having a class with a single responsibility of retrieving information about a token would be handy.
The [Windows Identity Foundation](https://msdn.microsoft.com/en-us/library/hh377151.aspx) in the .NET 4.5 framework supports a base class of [`SecurityToken`](https://msdn.microsoft.com/en-us/library/system.identitymodel.tokens.securitytoken(v=vs.110).aspx) for this purpose. Although there are several implementations available, I have created a few of my own for working with Azure and Auth0.

##Client-side Security

Even a cloud hosted service will still have a client which could abuse the system. Clients should receive the same consideration for security as the service you are building. The client application could be developed by another team (mobile for example), DevOps (ie. PowerShell scripts), or vendor. Make sure the threat models for the cloud service take these design choices into account. If a third party will be consuming the service try to limit their access, make sure tokens expire, support secure channels (ie. HTTPS), support encryption of sensitive data over the wire, etc. When a support ticket arises make sure it does not contain a token in plain text. Posting a Share Access Signature token that expires in a year on an open forum is a quick way to have a breach in security.

##Authentication and Authorization

Who am I? What do I have access to? When it comes to the cloud this can be a gray area. Are you an end-user or a service user? Can a user be both? With Micro-Services, I like to think there are three design types: 

#####1) User is authenticated and authorized with each service

Anytime a user sends a request to the service their authentication/authorization information is passed to each subsequent service. This technique is used when a cloud system (with multiple internal and external Micro-Services) requires fine grain access control and auditing per a user action. For example, we have a valid limited admin user Sally. She has access to the mobile application, Scheduler service, and Payment service. However, she doesn't have access to the Products (external) service for updating the cost of a product. Every time she logs into the mobile application she is authenticated and authorized per a action. She decides to create a new Schedule that hooks into their Payment service and displays the current product prices from the Product service. Sally's user is authenticated and her actions are authorized on each layer. Leaving an audit trail for her user. Now, Sally is tech savvy and manipulates her request to the scheduler service to change the cost of the product based on the time of day. When the scheduler runs it sends the request for setting the Product cost. The Product service will respond with unauthorized which is added to Sally's audit information within the Scheduler service. Poor Sally...


#####2) User authenticates with a service and a service authenticates with other services

Whenever a user sends a request to the service, the user authenticates with that specific service. The service will in turn authenticate with another service. A great example would be a user client sends a request to a Scheduler service to StartWork. The Scheduler service must authenticate with an Authorization service which checks if the user has the proper claims to perform the StartWork action. The end-user client should not require direct access to the Authorization service since it is an internal detail for that service.

#####3) User is authenticated and authorized at a central identity access management service

The first design does not fail fast. A user might have several service request hops before an unauthorized case is encountered. When the error scenario is encountered, the cloud system could go into an awkward state where the user's actions are now in a partial state. Recovering from the scenario is often not easy. In the second design, there is a user which could perform actions as a service. Imagine rather than having an Authorization service but a service which supports CRUD to business entities (ie. Products). A poor design but it could happen. In both cases, a "user" needs to be authenticated per a hop and requests are sent to the authorization service. Depending on the number of layers and Micro-Services this could have a negative impact on performance. Enter a central identity access management service. All user traffic is routed through the service which delegates authentication and authorization requests. Once the user is authorized the request is routed (using a routing service) to the appropriate Micro-Service which may call other services. Wow that's pretty cool! Wait a minute... Yes, there are several down-sides to a central identity access management service. 

1. The central identity access management service would need to know the inner workings of each service (ie. what requests are sent from a service to a given service). This often comes in the form of an authorization policy with claims.
2. Third party Micro-Services will not support it. They would require their own authentication and authorization.
3. Maintaining the service could cost more than implementing one of the other designs. The policies would need to be updated regularly and any changes to a micro-service might affect the authorization policy. 
4. Code complexity. 
5. Testing. Yes, a central access management service would place a huge burden on testing and their infrastructure.

Of course, it is possible for every service to automatically publish their individual policy actions and available internal services to the central service. Solving the first problem and helping with the third. Migrating user policies is quite entertaining...
A plugin hook for third party authentication and authorization is very feasible.

In short, a centralized identity access management service is possible. But most cloud systems will not see a benefit due to the complexity of the service. 


##Auditing

Auditing user actions in the cloud is a *must* because it provides accountability. A service could audit who is sending the request, the action performed, and the result of the action. Although auditing normally is for security purposes, the audit log can also help diagnosis service issues.


##Avoid keys to the Kingdom

I like to use the term "Keys to the Kingdom" when at any time a user or service is given elevated privileges due to having physical access to a secure key that does not expire. Azure infrastructure gives the tempting option to authenticate using a key in the connection string. As good practice, avoid using the Primary or Secondary key to authenticate with Azure services. Based on my experience with Service bus and Storage. Clients should utilize an expiring Share Access Signature token.

[Azure Shared Access Signature information](https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/)

[Azure Shared Access Signature Service Bus information](https://azure.microsoft.com/en-us/documentation/articles/service-bus-shared-access-signature-authentication/)


