import { NgModule } from '@angular/core';
import { environment } from '../../environments/environment';
import { CommonEntryComponent } from './common-entry.component';
import { FeedListComponent } from './feed-list/feed-list.component';
import { ApiModule } from './services/api/api.module';

@NgModule({
  imports: [ApiModule.forRoot({ rootUrl: environment.apiRootUrl })],
  declarations: [CommonEntryComponent, FeedListComponent],
  exports: [CommonEntryComponent]
})
export class RtfxCommonModule {}
