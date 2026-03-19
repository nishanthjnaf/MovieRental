import { TestBed } from '@angular/core/testing';

import { Genre } from './genre';

describe('Genre', () => {
  let service: Genre;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Genre);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
