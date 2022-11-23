import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { host } from '../environments/host';

const routes: Routes = [];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: host.electron })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
