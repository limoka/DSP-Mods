const fs = require('fs')
const path = require('path')
const readline = require("readline");
const toml = require('@iarna/toml')
var execFile = require('child_process').execFile;

const STAGE_FOLDER = 'Staging\\';
const CONFIG_FOLDER = 'Config\\';
const ARTIFACT_FOLDER = 'Build\\';
let SRC_FOLDER = 'BlueprintTweaks\\'
let PLUGIN_INFO = 'BlueprintTweaks\\BlueprintTweaksPlugin.cs'

let build = false;

function main() {
	
	if (process.argv.length >= 3){
		build = process.argv[2] != "release";
		
		if (process.argv.length >= 5) {
			SRC_FOLDER = process.argv[3];
			PLUGIN_INFO = path.join(SRC_FOLDER, process.argv[4]);
		}
	}
	if (build){
		console.log("Building:")
	}else{
		console.log("Building and publishing:")
	}
	
	console.log('Src path: ' + SRC_FOLDER + ', Info file: ' + PLUGIN_INFO);
	
	if (!fs.existsSync(ARTIFACT_FOLDER)) {
        fs.mkdirSync(ARTIFACT_FOLDER, {recursive: true})
    }
	
	const pluginInfo = getPluginInfo();
	generateManifest(pluginInfo)
	
	const rl = readline.createInterface({
		input: process.stdin,
		output: process.stdout
	});
	
	writeTOML(pluginInfo);
	
	if (!build) {
		rl.question("Are you sure you want to release Build version " + pluginInfo.version + " ?", function(ans) {
			if (ans.toLowerCase() == "y"){
				Publish();
			}else{
				console.log('Aborting!');
			}
			rl.close();
		});
	}else{
		Build();
		rl.close();
	}
}

function writeTOML(pluginInfo){

	const manifestPath = path.join(CONFIG_FOLDER, 'manifest.json');
	let manifest = JSON.parse(fs.readFileSync(manifestPath))
	
	const tsManifestPath = path.join(CONFIG_FOLDER, 'thunderstore.toml');
	var config = toml.parse(fs.readFileSync(tsManifestPath, 'utf-8'));
	var tsManifest = config["package"]
	tsManifest["name"] = pluginInfo.name;
	tsManifest["versionNumber"] = pluginInfo.version;
	
	tsManifest["description"] = manifest["description"];
	tsManifest["websiteUrl"] = manifest["website_url"];
	
	tsManifest["dependencies"] = {};
	
	manifest["dependencies"].forEach(mod => {
		let parts = mod.split('-');
		tsManifest["dependencies"][parts[0] + "-" + parts[1]] = parts[2];
	});
	
	config["package"] = tsManifest;
	const tomlOutput = toml.stringify(config)
	
	console.log('Writing thunderstore.toml');
	fs.writeFileSync(tsManifestPath, tomlOutput)
}

function Build(){
	
	//./tcli.exe build --config-path .\Config\thunderstore.toml
	execFile('./tcli.exe', ["build", "--config-path", ".\\Config\\thunderstore.toml"], (error, stdout, stderr) => {
		console.log(stdout);
	});
}

function Publish(){
	const tokenPath = path.join(CONFIG_FOLDER, 'tss_token.txt');
	const token = fs.readFileSync(tokenPath, 'utf8');
	
	//./tcli.exe publish --config-path .\Config\thunderstore.toml
	execFile('./tcli.exe', ["publish", "--token", token, "--config-path", ".\\Config\\thunderstore.toml"], (error, stdout, stderr) => {
		console.log(stdout);
	});
}

function getPluginInfo() {
    const pluginInfoRaw = fs.readFileSync(PLUGIN_INFO).toString("utf-8")
    return {
        name: pluginInfoRaw.match(/MODNAME = "(.*)";/)[1],
        id: pluginInfoRaw.match(/MODGUID = "(.*)";/)[1],
        version: pluginInfoRaw.match(/VERSION = "(.*)";/)[1],
    }
}

function generateManifest(pluginInfo) {
	
	const manifestPath = path.join(CONFIG_FOLDER, 'manifest.json');
	
	let manifest = JSON.parse(fs.readFileSync(manifestPath))
	
	manifest["name"] = pluginInfo.name;
	manifest["version_number"] = pluginInfo.version;
	
    fs.writeFileSync(path.join(CONFIG_FOLDER, 'manifest.json'), JSON.stringify(manifest, null, 2))
}

main();