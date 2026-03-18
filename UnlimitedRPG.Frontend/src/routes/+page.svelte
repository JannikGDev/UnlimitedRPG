<script lang="ts">
	import { goto } from '$app/navigation';
	import { getWorlds, createSession } from '$lib/api';
	import type { WorldDto } from '$lib/api';

	let worlds = $state<WorldDto[]>([]);
	let selected = $state<WorldDto | null>(null);
	let playerName = $state('');
	let loading = $state(true);
	let starting = $state(false);
	let error = $state('');

	$effect(() => {
		getWorlds()
			.then((w) => (worlds = w))
			.catch(() => (error = 'Could not reach the server. Is the API running?'))
			.finally(() => (loading = false));
	});

	async function beginAdventure() {
		if (!selected || !playerName.trim()) return;
		starting = true;
		error = '';
		try {
			const session = await createSession(selected.id, playerName.trim());
			goto(`/session/${session.sessionId}`);
		} catch {
			error = 'Failed to start session.';
			starting = false;
		}
	}
</script>

<main class="flex min-h-screen flex-col items-center justify-center px-4 py-16">
	<!-- Title -->
	<div class="mb-12 text-center">
		<h1 class="mb-2 text-5xl font-bold tracking-widest text-amber-400 uppercase">
			⚔ UnlimitedRPG
		</h1>
		<p class="text-sm tracking-wider text-zinc-400 uppercase">A modular roleplaying framework</p>
	</div>

	<div class="w-full max-w-lg space-y-8">
		<!-- World selection -->
		<section>
			<h2 class="mb-3 text-xs font-semibold tracking-widest text-zinc-400 uppercase">
				Choose a world
			</h2>

			{#if loading}
				<p class="text-sm text-zinc-500">Loading worlds…</p>
			{:else if worlds.length === 0 && !error}
				<p class="text-sm text-zinc-500">No worlds found.</p>
			{:else}
				<div class="grid grid-cols-2 gap-3">
					{#each worlds as world}
						<button
							onclick={() => (selected = world)}
							class="rounded border px-4 py-3 text-left text-sm font-medium transition-colors
								{selected?.id === world.id
								? 'border-amber-500 bg-amber-500/10 text-amber-300'
								: 'border-zinc-700 bg-zinc-900 text-zinc-200 hover:border-zinc-500 hover:bg-zinc-800'}"
						>
							{world.name}
						</button>
					{/each}
				</div>
			{/if}
		</section>

		<!-- Player name -->
		<section>
			<h2 class="mb-3 text-xs font-semibold tracking-widest text-zinc-400 uppercase">
				Your name
			</h2>
			<input
				type="text"
				placeholder="Enter character name…"
				bind:value={playerName}
				class="w-full rounded border border-zinc-700 bg-zinc-900 px-4 py-2 text-zinc-100
					placeholder-zinc-600 outline-none transition-colors
					focus:border-amber-500 focus:ring-1 focus:ring-amber-500/30"
			/>
		</section>

		<!-- Error -->
		{#if error}
			<p class="text-sm text-red-400">{error}</p>
		{/if}

		<!-- CTA -->
		<button
			onclick={beginAdventure}
			disabled={!selected || !playerName.trim() || starting}
			class="w-full rounded border border-amber-600 bg-amber-600/10 py-3 text-sm font-bold
				tracking-widest text-amber-400 uppercase transition-colors
				hover:bg-amber-600/20 disabled:cursor-not-allowed disabled:opacity-30"
		>
			{starting ? 'Starting…' : 'Begin Adventure'}
		</button>
	</div>
</main>
