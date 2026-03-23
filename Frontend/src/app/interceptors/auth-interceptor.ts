import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { retry, timer, throwError, catchError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {

  const token =
  localStorage.getItem('token') ||
  sessionStorage.getItem('token');

  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  const handle = (r: typeof authReq) => next(r).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 429) {
        return throwError(() => new Error('Too many requests. Please wait a moment and try again.'));
      }
      return throwError(() => err);
    })
  );

  if (authReq.method === 'GET') {
    return handle(authReq).pipe(
      retry({ count: 1, delay: () => timer(300) })
    );
  }
  return handle(authReq);
};