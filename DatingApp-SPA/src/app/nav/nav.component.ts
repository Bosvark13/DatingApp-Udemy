import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
    selector: 'app-nav',
    templateUrl: './nav.component.html',
    styleUrls: ['./nav.component.scss']
})
/** nav component*/
export class NavComponent implements OnInit {
  model: any = {};

  ngOnInit(): void {
        
  }

  /** nav ctor */
  constructor(public authService: AuthService, private alertify: AlertifyService) {

  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error('Failed to login');
    });
  }

  loggedIn() {
    return this.authService.loggedIn();
  }

  logOut() {
    localStorage.removeItem('token');

    this.alertify.message('Logged out');
  }
}
