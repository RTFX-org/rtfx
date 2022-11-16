import { exec } from 'child_process';
import * as fse from 'fs-extra';
import { exit } from 'process';

const execAsync = (command: string) => {
  return new Promise<void>((resolve, reject) => {
    exec(command, (error) => {
      if (error) {
        reject(error);
      } else {
        resolve();
      }
    });
  });
};

const createDirLink = async (
  linkPath: string,
  realPath: string
): Promise<void> => {
  if (fse.existsSync(linkPath)) {
    console.log(`Link path ${linkPath} already exists`);
    return;
  }
  await fse.ensureSymlink(realPath, linkPath, 'dir');
  console.log(`Created link ${linkPath} -> ${realPath}`);
};

createDirLink('./electron/rtfx', './dist/rtfx').catch((error) => {
  console.error(error);
  exit(1);
});
