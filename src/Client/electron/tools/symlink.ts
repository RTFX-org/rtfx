import { exec } from 'child_process';
import { platform } from 'os';
import * as fs from 'fs-extra';
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
  if (platform() === 'win32') {
    const cmd = `mklink /J "${linkPath}" "${realPath}"`;
    execAsync(cmd).then(() => {
      console.log(`Created link ${linkPath} -> ${realPath}`);
    });
  } else {
    await fs.ensureSymlink(realPath, linkPath, 'file');
    console.log(`Created link ${linkPath} -> ${realPath}`);
  }
};

fs.remove('./electron/rtfx').then(() => {
  createDirLink('./electron/rtfx', './dist/rtfx').catch((error) => {
    console.error(error);
    exit(1);
  });
});
