import { Component, OnInit } from '@angular/core';
import { FeedsService } from '../services/api/services';

@Component({
  selector: 'rtfx-feed-list',
  templateUrl: './feed-list.component.html',
  styleUrls: ['./feed-list.component.scss']
})
export class FeedListComponent implements OnInit {
  private readonly _feedsService: FeedsService;

  constructor(feedsService: FeedsService) {
    this._feedsService = feedsService;
  }

  ngOnInit(): void {
    this._feedsService.listFeedsEndpoint().then(feeds => {
      console.log(feeds);
    });
  }
}
