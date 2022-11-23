import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { WebBaseComponent } from './web-base.component';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RtfxCommonModule } from '../common/common.module';

@NgModule({
  imports: [
    SharedModule,
    RouterModule.forChild([{ path: '', component: WebBaseComponent }]),
    RtfxCommonModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule
  ],
  declarations: [WebBaseComponent]
})
export class WebBaseModule {}
