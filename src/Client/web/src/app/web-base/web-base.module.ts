import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { WebBaseComponent } from './web-base.component';

@NgModule({
  imports: [SharedModule, RouterModule.forChild([{ path: '', component: WebBaseComponent }])],
  declarations: [WebBaseComponent]
})
export class WebBaseModule {}
