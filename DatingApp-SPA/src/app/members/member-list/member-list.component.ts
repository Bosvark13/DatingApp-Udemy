import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';

@Component({
    selector: 'app-member-list',
    templateUrl: './member-list.component.html',
    styleUrls: ['./member-list.component.scss']
})
/** member-list component*/
export class MemberListComponent implements OnInit {    
  users: User[];
  user: User = JSON.parse(localStorage.getItem('user'));

  genderList = [{ value: 'male', display: 'Male' }, { value: 'female', display: 'Female' }];

  userParams: any = {};

  pagination: Pagination;

  /** member-list ctor */
  constructor(private userService: UserService, private route: ActivatedRoute, private alertify: AlertifyService) {

  }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.users = data['users'].results;
      this.pagination = data['users'].pagination;
    });

    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  resetFilter() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';

    this.loadUsers();
  }

  loadUsers() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
      .subscribe((result: PaginatedResult<User[]>) => {
        this.users = result.results;
        this.pagination = result.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }
}
