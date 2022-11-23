import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NgLetModule } from 'ng-let';

@NgModule({
  imports: [CommonModule, NgLetModule],
  declarations: [],
  exports: [CommonModule, NgLetModule]
})
export class SharedModule {}
