import { NgModule } from '@angular/core';
import { CommonEntryComponent } from './common-entry.component';
import { FeedListComponent } from './feed-list/feed-list.component';

@NgModule({
  imports: [],
  declarations: [CommonEntryComponent, FeedListComponent],
  exports: [CommonEntryComponent]
})
export class RtfxCommonModule {}
