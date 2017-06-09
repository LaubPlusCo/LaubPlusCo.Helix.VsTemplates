module.exports = function() {
  var instanceRoot = "!!$instanceRoot$!!";
  var config = {
    websiteRoot: instanceRoot + "\\Website",
    sitecoreLibraries: instanceRoot + "\\Website\\bin",
    licensePath: instanceRoot + "!!$licensefile$!!",
    solutionName: "$safeprojectname$",
    buildConfiguration: "Debug",
    runCleanBuilds: false
  };
  return config;
}