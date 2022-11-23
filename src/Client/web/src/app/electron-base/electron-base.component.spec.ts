import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ElectronBaseComponent } from './electron-base.component';

describe('ElectronBaseComponent', () => {
  let component: ElectronBaseComponent;
  let fixture: ComponentFixture<ElectronBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ElectronBaseComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ElectronBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
