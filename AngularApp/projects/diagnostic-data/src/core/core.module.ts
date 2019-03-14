import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VerticalNavComponent } from './vertical-nav/vertical-nav.component';
import { VerticalNavTabComponent } from './vertical-nav-tab/vertical-nav-tab.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [VerticalNavComponent, VerticalNavTabComponent],
  exports: [VerticalNavComponent, VerticalNavTabComponent]
})
export class CoreModule { }
