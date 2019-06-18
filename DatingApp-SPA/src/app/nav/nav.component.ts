import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-nav',
    templateUrl: './nav.component.html',
    styleUrls: ['./nav.component.scss']
})
/** nav component*/
export class NavComponent implements OnInit {
  model: any = {};
  photoUrl: string;

  ngOnInit(): void {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);      
  }

  /** nav ctor */
  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) {

  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error('Failed to login');
      }, () => {
        this.router.navigate(['/members']);
      });
  }

  loggedIn() {
    return this.authService.loggedIn();
  }

  logOut() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');

    this.authService.decodedToken = null;
    this.authService.currentUser = null;

    this.alertify.message('Logged out');

    this.router.navigate(['/home']);
  }
}
