import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { forkJoin, map, of, switchMap } from 'rxjs';
import { UNION_MEMBER_ROLE_LABELS, UNION_MEMBERSHIP_STATUS_LABELS, Union, UnionMember } from '../models';
import { UnionsService } from '../unions.service';

interface MembershipWithUnion {
  membership: UnionMember;
  union: Union;
}

@Component({
  selector: 'app-my-unions',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './my-unions.component.html',
  styleUrl: './my-unions.component.scss',
})
export class MyUnionsComponent {
  private readonly unionsService = inject(UnionsService);

  protected readonly roleLabels = UNION_MEMBER_ROLE_LABELS;
  protected readonly statusLabels = UNION_MEMBERSHIP_STATUS_LABELS;

  protected readonly membershipsResource = rxResource<MembershipWithUnion[], void>({
    stream: () =>
      this.unionsService.getMyMemberships().pipe(
        switchMap((memberships) =>
          memberships.length === 0
            ? of([])
            : forkJoin(
                memberships.map((membership) =>
                  this.unionsService.getById(membership.unionId).pipe(map((union) => ({ membership, union }))),
                ),
              ),
        ),
      ),
  });
}
