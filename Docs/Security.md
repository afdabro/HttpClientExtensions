#Security

Cloud security is a broad term that is often misrepresented. 
Yes, your data is in the cloud. A hacker *might* find it harder to retrieve your data simply because it is stored offsite. 
But cloud services still fall victim to the same low hanging fruit issues. Security is everyone’s responsibility. 
The [security development lifecycle (SDL)](https://www.microsoft.com/en-us/sdl/) should be coordinated within an [Agile development life cycle](http://www.ambysoft.com/essays/agileLifecycle.html). Design for Security. Build threat models. 
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

##Authentication

##Authorization

##Auditing

##Keys to the Kingdom
