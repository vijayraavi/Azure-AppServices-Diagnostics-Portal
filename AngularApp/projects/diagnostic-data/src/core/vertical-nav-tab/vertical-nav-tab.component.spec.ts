import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VerticalNavTabComponent } from './vertical-nav-tab.component';

describe('VerticalNavTabComponent', () => {
  let component: VerticalNavTabComponent;
  let fixture: ComponentFixture<VerticalNavTabComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VerticalNavTabComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VerticalNavTabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
