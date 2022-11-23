import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WebBaseComponent } from './web-base.component';

describe('WebBaseComponent', () => {
  let component: WebBaseComponent;
  let fixture: ComponentFixture<WebBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ WebBaseComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WebBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
