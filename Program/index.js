const { exec } = require('child_process');
const highlight = require('cli-highlight').highlight
const sqlFormatter = require('sql-formatter');

exec("dotnet run | awk '!/warning/'", (err, stdout, stderr) => {
  if (err) {
    console.log(err);
    // node couldn't execute the command
    return;
  }

  // the *entire* stdout and stderr (buffered)
  // console.log("\n\n"+highlight(sqlFormatter.format(stdout), { language: 'sql', ignoreIllegals: true }))
  console.log("\n\n"+highlight(stdout, { language: 'sql', ignoreIllegals: true }))
  //   console.log(`stderr: ${stderr}`);
});