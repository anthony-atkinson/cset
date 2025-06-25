//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for Angular configuration management in CSET.
    /// This controller handles dynamic configuration generation for the Angular frontend,
    /// supporting URL rewriting, connection string management, and runtime configuration
    /// updates. Enables dynamic configuration based on deployment environment.
    /// </summary>
    [ApiController]
    public class AngularConfigController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHost;

        /// <summary>
        /// Initializes a new instance of the AngularConfigController.
        /// </summary>
        /// <param name="webHost">The web hosting environment for file system access</param>
        public AngularConfigController(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        // SECURITY NOTE:  The following two endpoints should not be included in a Release build

#if !EXCLUDE_FROM_PUBLISH

        /// <summary>
        /// Changes the database connection string in appsettings.json.
        /// 
        /// SECURITY WARNING: This endpoint exposes sensitive configuration data and should
        /// only be used in development/testing environments. It is excluded from production
        /// builds via the EXCLUDE_FROM_PUBLISH preprocessor directive.
        /// </summary>
        /// <param name="connString">The new connection string to set</param>
        /// <returns>
        /// 200 OK with the previous connection string if successful
        /// Error message if the operation fails
        /// </returns>
        /// <remarks>
        /// This endpoint modifies the database connection string in appsettings.json:
        /// - Reads the current appsettings.json file
        /// - Updates the CSET_DB connection string
        /// - Writes the modified configuration back to disk
        /// - Returns the previous connection string value
        /// 
        /// Security considerations:
        /// - Only available in development builds
        /// - Exposes sensitive database credentials
        /// - Should not be used in production environments
        /// - Requires proper authentication in production
        /// 
        /// Usage scenarios:
        /// - Development environment configuration
        /// - Testing environment setup
        /// - Database connection troubleshooting
        /// - Configuration management automation
        /// 
        /// File operations:
        /// - Reads from appsettings.json in current directory
        /// - Updates ConnectionStrings:CSET_DB value
        /// - Preserves other configuration settings
        /// - Handles file I/O errors gracefully
        /// 
        /// This endpoint is excluded from production builds for security reasons.
        /// </remarks>
        [HttpPost]
        [Route("api/assets/changeconnectionstring")]
        [ProducesResponseType(200)]
        public string ChangeConnectionString([FromBody] string connString)
        {
            try
            {
                string currDirectory = Directory.GetCurrentDirectory();
                string appSettingsPath = currDirectory + "\\appsettings.json";

                if (System.IO.File.Exists(appSettingsPath))
                {
                    JObject document = JObject.Parse(System.IO.File.ReadAllText(appSettingsPath));
                    JToken element = document["ConnectionStrings"];

                    string previousConnString = element["CSET_DB"].ToString();
                    element["CSET_DB"] = connString;
                    document["ConnectionStrings"].Replace(element);

                    System.IO.File.WriteAllText(appSettingsPath, document.ToString());

                    return previousConnString;
                }
                else
                {
                    return "Error: \"appsettings.json\" could not be found";
                }
            }
            catch (Exception ex)
            {
                return "Error: something went wrong with changing the connection string in \"appsettings.json\". " + ex.Message;
            }
        }

        /// <summary>
        /// Retrieves the current database connection string from appsettings.json.
        /// 
        /// SECURITY WARNING: This endpoint exposes sensitive configuration data and should
        /// only be used in development/testing environments. It is excluded from production
        /// builds via the EXCLUDE_FROM_PUBLISH preprocessor directive.
        /// </summary>
        /// <returns>
        /// 200 OK with the current connection string if successful
        /// Error message if the operation fails
        /// </returns>
        /// <remarks>
        /// This endpoint reads the database connection string from appsettings.json:
        /// - Reads the current appsettings.json file
        /// - Extracts the CSET_DB connection string value
        /// - Returns the connection string for inspection
        /// - Handles file I/O errors gracefully
        /// 
        /// Security considerations:
        /// - Only available in development builds
        /// - Exposes sensitive database credentials
        /// - Should not be used in production environments
        /// - Requires proper authentication in production
        /// 
        /// Usage scenarios:
        /// - Development environment debugging
        /// - Configuration verification
        /// - Database connection troubleshooting
        /// - Configuration management automation
        /// 
        /// File operations:
        /// - Reads from appsettings.json in current directory
        /// - Extracts ConnectionStrings:CSET_DB value
        /// - Handles missing file scenarios
        /// - Provides detailed error messages
        /// 
        /// This endpoint is excluded from production builds for security reasons.
        /// </remarks>
        [HttpGet]
        [Route("api/assets/getconnectionstring")]
        [ProducesResponseType(200)]
        public string GetConnectionString()
        {
            try
            {
                string currDirectory = Directory.GetCurrentDirectory();
                string appSettingsPath = currDirectory + "\\appsettings.json";

                if (System.IO.File.Exists(appSettingsPath))
                {
                    JObject document = JObject.Parse(System.IO.File.ReadAllText(appSettingsPath));
                    JToken element = document["ConnectionStrings"];

                    return element["CSET_DB"].ToString();
                }
                else
                {
                    return "Error: \"appsettings.json\" could not be found";
                }
            }
            catch (Exception ex)
            {
                return "Error: something went wrong with getting the connection string in \"appsettings.json\". " + ex.Message;
            }
        }

#endif

        /// <summary>
        /// Retrieves the Angular configuration with dynamic URL rewriting.
        /// </summary>
        /// <returns>
        /// 200 OK with modified config.json containing updated URLs
        /// 400 Bad Request if config.json file is not found
        /// </returns>
        /// <remarks>
        /// This endpoint provides dynamic Angular configuration with URL rewriting:
        /// - Reads the base config.json file
        /// - Updates URLs to match the current deployment environment
        /// - Handles both integrated and separate deployments
        /// - Supports proxy and load balancer scenarios
        /// 
        /// The configuration process includes:
        /// - Detection of deployment mode (integrated vs separate)
        /// - Dynamic URL rewriting based on current host/port
        /// - Protocol detection (HTTP/HTTPS)
        /// - Port handling for standard and custom ports
        /// 
        /// Configuration features:
        /// - Dynamic host and port detection
        /// - Protocol-aware URL generation
        /// - Proxy header support (X-Forwarded-Proto, X-Forwarded-Port)
        /// - Fallback configuration handling
        /// - Integrated deployment support
        /// 
        /// URL rewriting includes:
        /// - App URL (frontend application)
        /// - API URL (backend services)
        /// - Library URL (documentation and resources)
        /// - Document URL (static assets)
        /// 
        /// Deployment scenarios:
        /// - Integrated deployment (API and frontend together)
        /// - Separate deployment (API and frontend apart)
        /// - Proxy/load balancer deployment
        /// - Development environment
        /// 
        /// The response includes:
        /// - Updated configuration with current URLs
        /// - Protocol and port information
        /// - Host information
        /// - Rewrite indicator flag
        /// 
        /// This endpoint is used by the Angular frontend to obtain
        /// runtime configuration for API endpoints and resources.
        /// 
        /// No authentication required - this is a public configuration endpoint.
        /// </remarks>
        [HttpGet]
        [Route("api/assets/config")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult GetConfigURLRewrite()
        {
            try
            {
                Console.WriteLine("Reading the path test");
                if (System.IO.File.Exists(Path.Combine(_webHost.ContentRootPath, "WebApp/index.html")))
                {
                    Console.WriteLine(Path.Combine(_webHost.ContentRootPath, "WebApp/index.html"));

                    //process this as if we are running internally else do what ever used to be the case
                    //in this case they are running together and we can just replace the config document. 
                    var jd = ProcessUpdatedJson(HttpContext.Request);
                    return Ok(jd);
                }
                Console.WriteLine("Path didn't exist");

                return Ok(ProcessConfig(HttpContext.Request.Host, HttpContext.Request.Scheme));
            }
            catch (Exception)
            {
                return BadRequest("assets/config.json file not found");
            }
        }

        /// <summary>
        /// Processes the configuration JSON for integrated deployment scenarios.
        /// </summary>
        /// <param name="context">The HTTP request context for URL information</param>
        /// <returns>Modified JObject with updated configuration</returns>
        /// <exception cref="Exception">Thrown when config file cannot be found</exception>
        private JObject ProcessUpdatedJson(HttpRequest context)
        {
            string webpath = _webHost.ContentRootPath;
            if (!webpath.Contains("WebApp"))
            {
                webpath = Path.Combine(_webHost.ContentRootPath, "WebApp");
            }

            var path = Path.Combine(webpath, "assets", "settings", "config.json");
            //if the files are there then assume we are running together
            //replace and return it. 

            if (System.IO.File.Exists(path))
            {
                JObject document = JObject.Parse(System.IO.File.ReadAllText(path));

                document.Add("rewrittenByRedirect", "true");

                JToken element = document["app"];
                element["host"] = context.Host.Host;
                if (String.IsNullOrWhiteSpace(context.Headers["X-Forwarded-Proto"]))
                {
                    element["protocol"] = context.Scheme;
                    string port = "443";
                    if ((context.Host.Port == 80) || (context.Host.Port == 443))
                        port = "";
                    else
                        port = (context.Host.Port == null) ? "" : context.Host.Port.ToString();
                    element["port"] = port;
                }
                else
                {
                    element["protocol"] = context.Headers["X-Forwarded-Proto"].ToString();
                    element["port"] = context.Headers["X-Forwarded-Port"].ToString();
                }

                element = document["api"];
                element["host"] = context.Host.Host;
                if (String.IsNullOrWhiteSpace(context.Headers["X-Forwarded-Proto"]))
                {
                    element["protocol"] = context.Scheme;
                    string port = "443";
                    if ((context.Host.Port == 80) || (context.Host.Port == 443))
                        port = "";
                    else
                        port = (context.Host.Port == null) ? "" : context.Host.Port.ToString();
                    element["port"] = port;
                }
                else
                {
                    element["protocol"] = context.Headers["X-Forwarded-Proto"].ToString();
                    element["port"] = context.Headers["X-Forwarded-Port"].ToString();
                }

                element = document["library"];
                element["host"] = context.Host.Host;
                if (String.IsNullOrWhiteSpace(context.Headers["X-Forwarded-Proto"]))
                {
                    element["protocol"] = context.Scheme;
                    string port = "443";
                    if ((context.Host.Port == 80) || (context.Host.Port == 443))
                        port = "";
                    else
                        port = (context.Host.Port == null) ? "" : context.Host.Port.ToString();
                    element["port"] = port;
                }
                else
                {
                    element["protocol"] = context.Headers["X-Forwarded-Proto"].ToString();
                    element["port"] = context.Headers["X-Forwarded-Port"].ToString();
                }

                Console.Write(document.ToString());
                return document;
            }
            throw new Exception("Cannot Find config file" + path);
        }

        /// <summary>
        /// Processes the configuration JSON for separate deployment scenarios.
        /// </summary>
        /// <param name="newBase">The host information for URL generation</param>
        /// <param name="scheme">The protocol scheme (HTTP/HTTPS)</param>
        /// <returns>Modified JsonElement with updated configuration</returns>
        /// <exception cref="Exception">Thrown when config file cannot be found</exception>
        private JsonElement ProcessConfig(HostString newBase, string scheme)
        {
            _webHost.WebRootPath = Path.Combine(_webHost.ContentRootPath, "../../../CSETWebNg/src");
            var path = Path.Combine(_webHost.WebRootPath, "assets/settings/config.json");
            if (System.IO.File.Exists(path))
            {
                string contents = System.IO.File.ReadAllText(path);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Utf8JsonWriter writer = new Utf8JsonWriter(memoryStream))
                    {
                        using (JsonDocument jDoc = JsonDocument.Parse(contents))
                        {
                            JsonElement root = jDoc.RootElement.Clone();

                            JsonElement overrideVal;
                            if (root.TryGetProperty("override", out overrideVal) != false)
                                if (overrideVal.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                    return root;

                            // get the base appURL 
                            // then change it to include the new port. 
                            string findString = root.GetProperty("app").GetProperty("url").ToString();
                            string replaceString = newBase + "/";

                            if (findString.SequenceEqual(replaceString))
                                return root;

                            // to edit json values, have to create an entire new JsonDocument since they are read-only
                            writer.WriteStartObject();
                            foreach (var element in root.EnumerateObject())
                            {
                                if (element.Name == "appUrl")
                                {
                                    writer.WritePropertyName(element.Name);
                                    writer.WriteStringValue(NewUri(newBase, scheme, root.GetProperty("appUrl").ToString()).ToString());
                                }
                                else if (element.Name == "apiUrl")
                                {
                                    writer.WritePropertyName(element.Name);
                                    writer.WriteStringValue(NewUri(newBase, scheme, root.GetProperty("apiUrl").ToString()).ToString());
                                }
                                else if (element.Name == "docUrl")
                                {
                                    writer.WritePropertyName(element.Name);
                                    writer.WriteStringValue(NewUri(newBase, scheme, root.GetProperty("docUrl").ToString()).ToString());
                                }
                                // write same value as original config json
                                else
                                {
                                    element.WriteTo(writer);
                                }
                            }
                            writer.WriteEndObject();
                        }
                        // create new JsonDocument with edited values
                        writer.Flush();
                        string newJson = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                        using JsonDocument newJDoc = JsonDocument.Parse(newJson);
                        return newJDoc.RootElement.Clone();
                    }
                }
            }
            throw new Exception("assets/config.json file not found");
        }

        /// <summary>
        /// Creates a new URI with updated host, port, and scheme information.
        /// </summary>
        /// <param name="newBase">The host information for the new URI</param>
        /// <param name="scheme">The protocol scheme (HTTP/HTTPS)</param>
        /// <param name="oldUri">The original URI to modify</param>
        /// <returns>New URI with updated host, port, and scheme</returns>
        private Uri NewUri(HostString newBase, string scheme, string oldUri)
        {
            //set the hostname and port to the same as the new base return the new uri
            UriBuilder tmp = new UriBuilder(oldUri);
            tmp.Host = newBase.Host;
            if ((newBase.Port == 80) || (newBase.Port == 443))
                tmp.Port = -1;
            else
                tmp.Port = newBase.Port ?? 80;
            tmp.Scheme = scheme;

            return tmp.Uri;
        }
    }
}
