import { Component } from '@angular/core';
import { ToastModule } from 'primeng/toast';
import { ShellComponent } from './core/layout/shell.component';

@Component({
  selector: 'app-root',
  imports: [ShellComponent, ToastModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
