<script lang="ts">
	import { goto } from '$app/navigation';
	import { onDestroy } from 'svelte';
	import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
	import { getSession, executeAction } from '$lib/api';
	import type { SessionStateDto } from '$lib/api';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let session = $state<SessionStateDto | null>(null);
	let acting = $state(false);
	let error = $state('');

	const hub = new HubConnectionBuilder()
		.withUrl('/hubs/content')
		.withAutomaticReconnect()
		.build();

	hub.on('NarrationReady', (_sessionId: string, round: number, narration: string) => {
		if (!session) return;
		console.log(session);
		session = {
			...session,
			combatLog: session.combatLog.map((e) =>
				e.round === round && e.provider === 'pending'
					? { ...e, narration, provider: 'stub' }
					: e
			)
		};
	});

	$effect(() => {
		getSession(data.id)
			.then((s) => {
				session = s;
				return hub.start();
			})
			.then(() => hub.invoke('JoinSession', data.id))
			.catch(() => (error = 'Failed to load session.'));
	});

	onDestroy(() => {
		if (hub.state !== HubConnectionState.Disconnected) hub.stop();
	});

	async function attack() {
		if (!session || session.status !== 'Active' || acting) return;
		acting = true;
		error = '';
		try {
			session = await executeAction(data.id, 'attack');
		} catch {
			error = 'Action failed.';
		} finally {
			acting = false;
		}
	}

	function hpPercent(current: number, max: number) {
		return Math.max(0, Math.min(100, (current / max) * 100));
	}

	const statusColour: Record<string, string> = {
		Alive: 'text-green-400 border-green-700 bg-green-900/30',
		Staggered: 'text-yellow-400 border-yellow-700 bg-yellow-900/30',
		Dead: 'text-red-400 border-red-700 bg-red-900/30'
	};
</script>

{#if !session && !error}
	<div class="flex min-h-screen items-center justify-center text-zinc-500 text-sm">
		Loading session…
	</div>
{:else if error && !session}
	<div class="flex min-h-screen items-center justify-center text-red-400 text-sm">{error}</div>
{:else if session}
	<div class="flex min-h-screen flex-col">
		<!-- Header -->
		<header class="flex items-center justify-between border-b border-zinc-800 px-6 py-3">
			<button onclick={() => goto('/')} class="text-xs tracking-widest text-zinc-500 uppercase hover:text-zinc-300 transition-colors">
				← UnlimitedRPG
			</button>
			<div class="flex items-center gap-3">
				<span class="text-xs text-zinc-400">Round {session.round}</span>
				<span class="rounded border px-2 py-0.5 text-xs font-medium tracking-wider uppercase
					{session.status === 'Active' ? 'border-green-800 text-green-400' :
					 session.status === 'Completed' ? 'border-amber-700 text-amber-400' :
					 'border-zinc-700 text-zinc-400'}">
					{session.status}
				</span>
			</div>
		</header>

		<!-- Outcome banner -->
		{#if session.status === 'Completed'}
			<div class="border-b border-amber-800 bg-amber-900/20 px-6 py-4 text-center">
				<p class="text-lg font-bold tracking-widest text-amber-400 uppercase">⚔ Victory!</p>
				<p class="mt-1 text-sm text-zinc-400">The enemy has been defeated.</p>
			</div>
		{:else if session.status === 'Abandoned'}
			<div class="border-b border-red-900 bg-red-900/20 px-6 py-4 text-center">
				<p class="text-lg font-bold tracking-widest text-red-400 uppercase">☠ Defeated</p>
				<p class="mt-1 text-sm text-zinc-400">Your adventure ends here.</p>
			</div>
		{/if}

		<!-- Combat arena -->
		<div class="flex flex-1 gap-4 p-6">

			<!-- Player panel -->
			<div class="w-52 shrink-0 rounded border border-zinc-800 bg-zinc-900 p-4">
				<p class="mb-3 text-xs font-semibold tracking-widest text-zinc-400 uppercase">Player</p>
				<p class="mb-3 text-base font-bold text-zinc-100">{session.player.name}</p>

				<p class="mb-1 text-xs text-zinc-500">
					HP — {session.player.currentHp} / {session.player.maxHp}
				</p>
				<div class="mb-4 h-2 rounded bg-zinc-700">
					<div
						class="h-2 rounded bg-green-500 transition-all"
						style="width: {hpPercent(session.player.currentHp, session.player.maxHp)}%"
					></div>
				</div>

				<div class="space-y-1 text-xs">
					<div class="flex justify-between">
						<span class="text-zinc-500">Attack</span>
						<span class="text-zinc-200 font-mono">+{session.player.attackBonus}</span>
					</div>
					<div class="flex justify-between">
						<span class="text-zinc-500">Damage</span>
						<span class="text-zinc-200 font-mono">+{session.player.damageBonus}</span>
					</div>
					<div class="flex justify-between">
						<span class="text-zinc-500">Armour</span>
						<span class="text-zinc-200 font-mono">{session.player.armorClass}</span>
					</div>
				</div>
			</div>

			<!-- Combat log -->
			<div class="flex flex-1 flex-col rounded border border-zinc-800 bg-zinc-900">
				<p class="border-b border-zinc-800 px-4 py-2 text-xs font-semibold tracking-widest text-zinc-400 uppercase">
					Combat Log
				</p>

				<div class="flex flex-1 flex-col-reverse gap-3 overflow-y-auto p-4">
					{#if session.combatLog.length === 0}
						<p class="text-sm text-zinc-600 italic">The battle has not yet begun…</p>
					{:else}
						{#each [...session.combatLog].reverse() as entry}
							<div class="rounded border border-zinc-800 bg-zinc-950 p-3">
								<div class="mb-1 flex items-center gap-2">
									<span class="text-xs font-mono text-zinc-500">Round {entry.round}</span>
									{#if entry.hit}
										<span class="text-xs font-semibold text-green-400">Hit — {entry.damage} dmg</span>
									{:else}
										<span class="text-xs font-semibold text-zinc-500">Miss</span>
									{/if}
								</div>
								{#if entry.provider === 'pending'}
									<p class="text-xs text-zinc-600 italic">Narration incoming…</p>
								{:else if entry.narration}
									<p class="text-sm leading-relaxed text-zinc-300">{entry.narration}</p>
									<p class="mt-1 text-xs text-zinc-600">via {entry.provider}</p>
								{/if}
							</div>
						{/each}
					{/if}
				</div>

				<!-- Action bar -->
				<div class="border-t border-zinc-800 p-4">
					{#if error}
						<p class="mb-2 text-xs text-red-400">{error}</p>
					{/if}
					<button
						onclick={attack}
						disabled={session.status !== 'Active' || acting}
						class="w-full rounded border border-red-800 bg-red-900/20 py-3 text-sm font-bold
							tracking-widest text-red-300 uppercase transition-colors
							hover:bg-red-900/40 disabled:cursor-not-allowed disabled:opacity-30"
					>
						{acting ? 'Attacking…' : '⚔ Attack'}
					</button>
				</div>
			</div>

			<!-- Enemy panel -->
			<div class="w-52 shrink-0 rounded border border-zinc-800 bg-zinc-900 p-4">
				<p class="mb-3 text-xs font-semibold tracking-widest text-zinc-400 uppercase">Enemy</p>
				<p class="mb-1 text-base font-bold text-zinc-100">{session.enemy.name}</p>

				<span class="mb-3 inline-block rounded border px-2 py-0.5 text-xs font-medium tracking-wider uppercase
					{statusColour[session.enemy.status] ?? 'text-zinc-400 border-zinc-700'}">
					{session.enemy.status}
				</span>

				<p class="mb-1 mt-2 text-xs text-zinc-500">
					HP — {session.enemy.currentHp} / {session.enemy.maxHp}
				</p>
				<div class="mb-4 h-2 rounded bg-zinc-700">
					<div
						class="h-2 rounded bg-red-600 transition-all"
						style="width: {hpPercent(session.enemy.currentHp, session.enemy.maxHp)}%"
					></div>
				</div>

				<div class="space-y-1 text-xs">
					<div class="flex justify-between">
						<span class="text-zinc-500">Attack</span>
						<span class="text-zinc-200 font-mono">+{session.enemy.attackBonus}</span>
					</div>
					<div class="flex justify-between">
						<span class="text-zinc-500">Damage</span>
						<span class="text-zinc-200 font-mono">+{session.enemy.damageBonus}</span>
					</div>
					<div class="flex justify-between">
						<span class="text-zinc-500">Armour</span>
						<span class="text-zinc-200 font-mono">{session.enemy.armorClass}</span>
					</div>
				</div>
			</div>
		</div>
	</div>
{/if}
