import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Boarder } from '../models/boarder.model';
import { BoarderService } from './boarder.service';

@Injectable({
  providedIn: 'root'
})
export class BoarderResolverService implements Resolve<Boarder> {

  constructor(private readonly boarderService: BoarderService, private readonly router: Router) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Boarder | Observable<Boarder> | Promise<Boarder> {
    const id = BoarderResolverService.getIdFromRoute(route);
    if (typeof id === 'number') {
      return this.boarderService.details(id).pipe(catchError(error => {
        this.router.navigate(["/boarders/error404"], { skipLocationChange: true });
        return of(new Boarder());
      }));
    }
    return Promise.reject(`Failed to read boarder id from ${route.paramMap.get('id')}`);
  }

  private static getIdFromRoute(route: ActivatedRouteSnapshot): number | undefined {
    const routeId = route.paramMap.get('id');
    if (routeId) {
      return Number.parseInt(routeId);
    }
    return undefined;
  }
}
